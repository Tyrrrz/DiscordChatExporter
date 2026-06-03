#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
COMPOSE_FILE="${DCE_COMPOSE_FILE:-$REPO_ROOT/docker-compose.yml}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
HOST_RUNNER="${DCE_HOST_RUNNER:-$REPO_ROOT/scripts/run-discord-scrape-host.sh}"
DOCUMENTS_SCRAPE="${DCE_DOCUMENTS_SCRAPE:-$REPO_ROOT/scripts/run-documents-scrape.sh}"
CONFIG_FILE="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
LOG_FILE="${DCE_LOG_FILE:-$REPO_ROOT/logs/discord-scrape.log}"
JOB_NAME="discord-scrape"
INTERVAL="monthly"
RUN_AT="04:00"
CRON_EXPRESSION=""
DRY_RUN=0
REMOVE=0
SKIP_PREFLIGHT=0
SALVAGE_BEFORE=0

TARGETS=()
GUILDS=()
CHANNELS=()

JQ_BIN="${DCE_JQ_BIN:-jq}"
CRONTAB_BIN="${DCE_CRONTAB_BIN:-crontab}"
DOCKER_BIN="${DCE_DOCKER_BIN:-docker}"
COMPOSE_BIN="${DCE_COMPOSE_BIN:-}"
DOCKER_BIN_OVERRIDDEN=0

if [[ -n "${DCE_DOCKER_BIN:-}" ]]; then
  DOCKER_BIN_OVERRIDDEN=1
fi

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [options]

Options:
  --target NAME       Restrict the cron job to one configured target. Repeatable.
  --guild ID          Narrow a selected target to one of its allowed guild IDs. Repeatable.
  --channel ID        Narrow a selected target to one of its allowed channel IDs. Repeatable.
  --interval VALUE    monthly, weekly, or daily. Default: monthly
  --at HH:MM          Run time in 24-hour format. Default: 04:00
  --cron EXPR         Use an explicit five-field cron expression instead of --interval/--at.
  --job-name NAME     Marker name for the installed cron block. Default: discord-scrape
  --log-file PATH     Cron log file. Default: $LOG_FILE
  --config PATH       Scrape targets JSON. Default: $CONFIG_FILE
  --env-file PATH     Compose env file. Default: $ENV_FILE
  --salvage-before-scrape  Cron job merges stale .dce-temp exports before incremental scrape
  --skip-preflight    Install the cron job without running the authenticated container preflight.
  --dry-run           Print the cron block instead of installing it.
  --remove            Remove the managed cron block and exit.
  --help              Show this help text.

Examples:
  $(basename "$0")
  $(basename "$0") --target discord_dms --interval weekly --at 02:30
  $(basename "$0") --target KotOR_discord_msgs --channel 221726893064454144 --salvage-before-scrape
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_program() {
  local program_path=$1
  command -v "$program_path" >/dev/null 2>&1 || die "Required command '$program_path' is missing."
}

cron_from_schedule() {
  local interval=$1
  local run_at=$2
  local hour minute

  [[ "$run_at" =~ ^([0-1][0-9]|2[0-3]):([0-5][0-9])$ ]] || die "--at must use HH:MM in 24-hour time."
  hour=${BASH_REMATCH[1]}
  minute=${BASH_REMATCH[2]}

  case "$interval" in
    monthly) printf '%s %s 1 * *' "$minute" "$hour" ;;
    weekly)  printf '%s %s * * 0' "$minute" "$hour" ;;
    daily)   printf '%s %s * * *' "$minute" "$hour" ;;
    *) die "Unsupported --interval '$interval'. Use monthly, weekly, or daily." ;;
  esac
}

strip_existing_job() {
  local existing_crontab=$1
  local begin_marker=$2
  local end_marker=$3

  awk -v begin="$begin_marker" -v end="$end_marker" '
    $0 == begin { skipping = 1; next }
    $0 == end { skipping = 0; next }
    !skipping { print }
  ' <<<"$existing_crontab"
}

validate_cron_expression() {
  local expr=$1
  local -a fields=()
  local field

  read -r -a fields <<<"$expr"
  ((${#fields[@]} == 5)) || die "--cron must contain exactly five fields (minute hour day month weekday)."

  for field in "${fields[@]}"; do
    [[ -n "$field" ]] || die "Empty field in --cron expression."
    [[ "$field" =~ ^[0-9*,/-]+$ ]] || die "Invalid cron field '$field' in --cron expression."
  done
}

append_target_args() {
  local -n _out=$1

  local target
  for target in "${TARGETS[@]}"; do
    _out+=(--target "$target")
  done

  local guild_id
  for guild_id in "${GUILDS[@]}"; do
    _out+=(--guild "$guild_id")
  done

  local channel_id
  for channel_id in "${CHANNELS[@]}"; do
    _out+=(--channel "$channel_id")
  done
}

container_config_path() {
  local config_path=$1

  if [[ "$config_path" == "$REPO_ROOT/config/"* ]]; then
    printf '/config/%s\n' "$(basename "$config_path")"
    return 0
  fi

  if [[ "$config_path" == config/* ]]; then
    printf '/config/%s\n' "${config_path#config/}"
    return 0
  fi

  printf '%s\n' "$config_path"
}

ensure_target_directories() {
  local selected_targets_json archive_root output_dir

  archive_root=$("$JQ_BIN" -r '.archive_root // empty' "$CONFIG_FILE")
  [[ -n "$archive_root" ]] || die "Config is missing archive_root."
  mkdir -p "$archive_root"

  selected_targets_json=$("$JQ_BIN" -cn '$ARGS.positional' --args "${TARGETS[@]}")

  if (( ${#TARGETS[@]} == 0 )); then
    while IFS= read -r output_dir; do
      mkdir -p "$output_dir"
    done < <("$JQ_BIN" -r '.targets[] | select(.enabled != false) | .output_dir' "$CONFIG_FILE")
    return 0
  fi

  while IFS= read -r output_dir; do
    mkdir -p "$output_dir"
  done < <(
    "$JQ_BIN" -r \
      --argjson selected_targets "$selected_targets_json" \
      '.targets[]
        | select(.name as $name | $selected_targets | index($name))
        | .output_dir' \
      "$CONFIG_FILE"
  )
}

validate_targets() {
  (( ${#TARGETS[@]} == 0 )) && return 0

  local requested_targets_json resolved_count
  requested_targets_json=$("$JQ_BIN" -cn '$ARGS.positional' --args "${TARGETS[@]}")
  resolved_count=$(
    "$JQ_BIN" -r \
      --argjson requested_targets "$requested_targets_json" \
      '[.targets[] | select(.name as $name | $requested_targets | index($name))] | length' \
      "$CONFIG_FILE"
  )

  [[ "$resolved_count" == "${#TARGETS[@]}" ]] || die "One or more --target values are missing from $CONFIG_FILE."
}

run_preflight() {
  local -a preflight_args=()

  [[ -f "$ENV_FILE" ]] || die "Missing env file: $ENV_FILE"
  preflight_args=(
    "$HOST_RUNNER"
    --env-file "$ENV_FILE"
    --compose-file "$COMPOSE_FILE"
    preflight
    --config "$(container_config_path "$CONFIG_FILE")"
  )
  append_target_args preflight_args
  "${preflight_args[@]}"
}

main() {
  while (($#)); do
    case "$1" in
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        TARGETS+=("$2")
        shift 2
        ;;
      --guild)
        [[ $# -ge 2 ]] || die "Missing value for --guild."
        GUILDS+=("$2")
        shift 2
        ;;
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        CHANNELS+=("$2")
        shift 2
        ;;
      --interval)
        [[ $# -ge 2 ]] || die "Missing value for --interval."
        INTERVAL=$2
        shift 2
        ;;
      --at)
        [[ $# -ge 2 ]] || die "Missing value for --at."
        RUN_AT=$2
        shift 2
        ;;
      --cron)
        [[ $# -ge 2 ]] || die "Missing value for --cron."
        CRON_EXPRESSION=$2
        shift 2
        ;;
      --job-name)
        [[ $# -ge 2 ]] || die "Missing value for --job-name."
        JOB_NAME=$2
        shift 2
        ;;
      --log-file)
        [[ $# -ge 2 ]] || die "Missing value for --log-file."
        LOG_FILE=$2
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_FILE=$2
        shift 2
        ;;
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
        shift 2
        ;;
      --skip-preflight)
        SKIP_PREFLIGHT=1
        shift
        ;;
      --salvage-before-scrape)
        SALVAGE_BEFORE=1
        shift
        ;;
      --dry-run)
        DRY_RUN=1
        shift
        ;;
      --remove)
        REMOVE=1
        shift
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      *)
        die "Unknown option: $1"
        ;;
    esac
  done

  require_program "$JQ_BIN"
  require_program "$CRONTAB_BIN"
  if [[ -n "$COMPOSE_BIN" ]]; then
    require_program "$COMPOSE_BIN"
  elif (( DOCKER_BIN_OVERRIDDEN == 0 )) && command -v docker-compose >/dev/null 2>&1; then
    :
  else
    require_program "$DOCKER_BIN"
  fi

  [[ -f "$COMPOSE_FILE" ]] || die "Missing compose file: $COMPOSE_FILE"
  [[ -x "$HOST_RUNNER" ]] || die "Missing or non-executable host runner: $HOST_RUNNER"
  [[ -x "$DOCUMENTS_SCRAPE" ]] || die "Missing or non-executable documents scrape: $DOCUMENTS_SCRAPE"
  [[ -f "$CONFIG_FILE" ]] || die "Missing config file: $CONFIG_FILE"
  "$JQ_BIN" empty "$CONFIG_FILE" >/dev/null 2>&1 || die "Invalid JSON config: $CONFIG_FILE"

  validate_targets

  if (( (${#GUILDS[@]} > 0 || ${#CHANNELS[@]} > 0) && ${#TARGETS[@]} != 1 )); then
    die "--guild and --channel overrides require exactly one --target."
  fi

  local cron_line
  if [[ -n "$CRON_EXPRESSION" ]]; then
    validate_cron_expression "$CRON_EXPRESSION"
    cron_line=$CRON_EXPRESSION
  else
    cron_line=$(cron_from_schedule "$INTERVAL" "$RUN_AT")
  fi

  local begin_marker="# BEGIN ${JOB_NAME}"
  local end_marker="# END ${JOB_NAME}"
  local current_crontab cleaned_crontab scrape_command job_line lock_prefix
  local -a scrape_args=()
  current_crontab=$("$CRONTAB_BIN" -l 2>/dev/null || true)
  cleaned_crontab=$(strip_existing_job "$current_crontab" "$begin_marker" "$end_marker")

  if (( REMOVE == 1 )); then
    if (( DRY_RUN == 1 )); then
      printf '%s\n' "$cleaned_crontab"
      exit 0
    fi

    printf '%s\n' "$cleaned_crontab" | "$CRONTAB_BIN" -
    exit 0
  fi

  mkdir -p "$(dirname "$LOG_FILE")"
  ensure_target_directories

  if (( SKIP_PREFLIGHT == 0 )); then
    run_preflight
  fi

  scrape_args=(
    "$DOCUMENTS_SCRAPE"
    --config "$CONFIG_FILE"
    --log-file "$LOG_FILE"
  )
  append_target_args scrape_args
  if (( SALVAGE_BEFORE == 1 )); then
    scrape_args+=(--salvage-before-scrape)
  fi
  scrape_command=$(printf '%q ' "${scrape_args[@]}")
  if command -v flock >/dev/null 2>&1; then
    lock_prefix=$(printf '%q ' "$(command -v flock)" "-n" "/tmp/${JOB_NAME}.lock")
  else
    lock_prefix=""
  fi

  job_line="$cron_line cd $(printf '%q' "$REPO_ROOT") && DCE_COMPOSE_TTY=0 DCE_ENV_FILE=$(printf '%q' "$ENV_FILE") DCE_COMPOSE_FILE=$(printf '%q' "$COMPOSE_FILE") ${lock_prefix}${scrape_command}"

  local cron_block
  cron_block=$(printf '%s\n%s\n%s\n' "$begin_marker" "$job_line" "$end_marker")

  if (( DRY_RUN == 1 )); then
    printf '%s\n' "$cron_block"
    exit 0
  fi

  {
    if [[ -n "$cleaned_crontab" ]]; then
      printf '%s\n\n' "$cleaned_crontab"
    fi
    printf '%s\n' "$cron_block"
  } | "$CRONTAB_BIN" -
}

main "$@"
