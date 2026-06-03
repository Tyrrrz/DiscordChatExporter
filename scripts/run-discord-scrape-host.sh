#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"
COMPOSE_FILE="${DCE_COMPOSE_FILE:-$REPO_ROOT/docker-compose.yml}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
DOCKER_BIN="${DCE_DOCKER_BIN:-docker}"
COMPOSE_BIN="${DCE_COMPOSE_BIN:-}"
DOCKER_BIN_OVERRIDDEN=0
REAUTH_COMMAND=""
COMPOSE_ENV_FILE=""
COMPOSE_ENV_TEMP=""
VERIFY_READY="$REPO_ROOT/scripts/verify-operator-ready.sh"

if [[ -n "${DCE_DOCKER_BIN:-}" ]]; then
  DOCKER_BIN_OVERRIDDEN=1
fi

usage() {
  cat <<EOF
Usage:
  $(basename "$0") preflight [run-discord-scrape options...]
  $(basename "$0") scrape [run-discord-scrape options...]

Options:
  --env-file PATH      Env file to load and pass to compose. Default: $ENV_FILE
  --compose-file PATH  Compose file path. Default: $COMPOSE_FILE
  --help               Show this help text.

Environment:
  DISCORD_TOKEN            Direct token value (highest precedence after refresh).
  DISCORD_TOKEN_FILE       Optional path to a file containing the Discord token.
  DCE_REAUTH_COMMAND       Optional absolute path to an executable reauth script under the repo root.

Notes:
  When $ENV_FILE is missing, exported DISCORD_TOKEN or DISCORD_TOKEN_FILE is used instead.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_program() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

cleanup_compose_env() {
  if [[ -n "$COMPOSE_ENV_TEMP" && -f "$COMPOSE_ENV_TEMP" ]]; then
    rm -f "$COMPOSE_ENV_TEMP"
  fi
}

load_env_file() {
  [[ -f "$ENV_FILE" ]] || die "Missing env file: $ENV_FILE"
  local raw_line line key value

  while IFS= read -r raw_line || [[ -n "$raw_line" ]]; do
    line=$(printf '%s' "$raw_line" | sed -E 's/^[[:space:]]+//; s/[[:space:]]+$//')
    [[ -n "$line" ]] || continue
    if [[ "$line" == \#* ]]; then
      continue
    fi
    if [[ "$line" == export\ * ]]; then
      line=${line#export }
      line=$(printf '%s' "$line" | sed -E 's/^[[:space:]]+//')
    fi

    [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]] || die "Invalid env assignment in $ENV_FILE: $raw_line"

    key=${line%%=*}
    value=${line#*=}

    if [[ "$value" =~ ^\".*\"$ ]]; then
      value=${value:1:${#value}-2}
    elif [[ "$value" =~ ^\'.*\'$ ]]; then
      value=${value:1:${#value}-2}
    fi

    printf -v "$key" '%s' "$value"
    export "$key"
  done <"$ENV_FILE"
}

write_compose_env_temp() {
  COMPOSE_ENV_TEMP=$(mktemp "${TMPDIR:-/tmp}/dce-compose-env.XXXXXX")
  COMPOSE_ENV_FILE="$COMPOSE_ENV_TEMP"

  if [[ -n "${DISCORD_TOKEN:-}" ]]; then
    printf 'DISCORD_TOKEN=%s\n' "$DISCORD_TOKEN" >"$COMPOSE_ENV_TEMP"
  else
    : >"$COMPOSE_ENV_TEMP"
  fi
  if [[ -n "${DISCORD_TOKEN_FILE:-}" ]]; then
    printf 'DISCORD_TOKEN_FILE=%s\n' "$DISCORD_TOKEN_FILE" >>"$COMPOSE_ENV_TEMP"
  fi
  if [[ -n "${DCE_REAUTH_COMMAND:-}" ]]; then
    printf 'DCE_REAUTH_COMMAND=%s\n' "$DCE_REAUTH_COMMAND" >>"$COMPOSE_ENV_TEMP"
  fi
  if [[ -n "${DCE_USERNS_MODE:-}" ]]; then
    printf 'DCE_USERNS_MODE=%s\n' "$DCE_USERNS_MODE" >>"$COMPOSE_ENV_TEMP"
  fi
  if [[ -n "${DCE_UID:-}" ]]; then
    printf 'DCE_UID=%s\n' "$DCE_UID" >>"$COMPOSE_ENV_TEMP"
  fi
  if [[ -n "${DCE_GID:-}" ]]; then
    printf 'DCE_GID=%s\n' "$DCE_GID" >>"$COMPOSE_ENV_TEMP"
  fi
}

configure_rootless_compose() {
  if [[ -n "${DCE_USERNS_MODE:-}" ]]; then
    return 0
  fi

  if [[ "$DOCKER_BIN" == *podman* ]] || podman info >/dev/null 2>&1; then
    export DCE_USERNS_MODE=keep-id
  fi
}

prepare_compose_env() {
  if [[ -f "$ENV_FILE" ]]; then
    load_env_file
    if [[ -n "${DISCORD_TOKEN_FILE:-}" && -f "${DISCORD_TOKEN_FILE}" ]]; then
      load_token_from_file || true
    elif [[ -z "${DISCORD_TOKEN:-}" ]]; then
      discover_token_file || true
      load_token_from_file || true
      load_token_from_discover_script || true
    fi
    write_compose_env_temp
    configure_rootless_compose
    return 0
  fi

  if [[ -z "${DISCORD_TOKEN:-}" ]]; then
    discover_token_file || true
    load_token_from_file || true
    load_token_from_discover_script || true
  fi

  if [[ -n "${DISCORD_TOKEN:-}" || -n "${DISCORD_TOKEN_FILE:-}" ]]; then
    write_compose_env_temp
    configure_rootless_compose
    return 0
  fi

  die "Missing env file: $ENV_FILE (copy scrape.env.example to scrape.env), export DISCORD_TOKEN / DISCORD_TOKEN_FILE, or place a token at $REPO_ROOT/.discord-token or ~/.config/discord-scrape/token."
}

load_token_from_file() {
  local token_file=${DISCORD_TOKEN_FILE:-}
  [[ -n "$token_file" ]] || return 1
  [[ -f "$token_file" ]] || die "DISCORD_TOKEN_FILE does not exist: $token_file"

  local token_value
  token_value=$(head -n 1 "$token_file" | tr -d '\r')
  [[ -n "$token_value" ]] || die "DISCORD_TOKEN_FILE is empty: $token_file"
  export DISCORD_TOKEN="$token_value"
  return 0
}

discover_token_file() {
  local candidate

  if [[ -n "${DISCORD_TOKEN_FILE:-}" && -f "${DISCORD_TOKEN_FILE}" ]]; then
    return 0
  fi

  for candidate in \
    "$REPO_ROOT/.discord-token" \
    "$HOME/.config/discord-scrape/token" \
    "$HOME/.config/discord-token"; do
    if [[ -f "$candidate" ]]; then
      export DISCORD_TOKEN_FILE="$candidate"
      return 0
    fi
  done

  return 1
}

load_token_from_discover_script() {
  local discover_script="$REPO_ROOT/scripts/discover-discord-token.sh"
  local token_value

  [[ -x "$discover_script" ]] || return 1
  token_value=$("$discover_script" 2>/dev/null) || return 1
  [[ -n "$token_value" ]] || return 1
  export DISCORD_TOKEN="$token_value"
  return 0
}

ensure_token_present() {
  if [[ -n "${DISCORD_TOKEN_FILE:-}" && -f "${DISCORD_TOKEN_FILE}" ]]; then
    load_token_from_file || true
  elif [[ -z "${DISCORD_TOKEN:-}" ]]; then
    discover_token_file || true
    load_token_from_file || true
  fi
  if [[ -z "${DISCORD_TOKEN:-}" ]]; then
    load_token_from_discover_script || true
  fi
  [[ -n "${DISCORD_TOKEN:-}" ]] || die "DISCORD_TOKEN is not set. Set DISCORD_TOKEN or DISCORD_TOKEN_FILE in $ENV_FILE, export it in the shell, place a token at $REPO_ROOT/.discord-token or ~/.config/discord-scrape/token, or sign in via DiscordChatExporter GUI / Discord desktop on this machine."
}

resolve_compose_bin() {
  if [[ -n "${DCE_COMPOSE_BIN:-}" ]]; then
    COMPOSE_BIN=$DCE_COMPOSE_BIN
    return 0
  fi
  # Smoke tests inject DCE_DOCKER_BIN with a fake compose shim; never route those through podman-compose.
  if (( DOCKER_BIN_OVERRIDDEN == 1 )); then
    COMPOSE_BIN=""
    return 0
  fi
  if command -v podman-compose >/dev/null 2>&1 && podman info >/dev/null 2>&1; then
    COMPOSE_BIN=podman-compose
    return 0
  fi
  COMPOSE_BIN=""
}

compose_run_args() {
  local -n _out=$1
  local subcommand=$2
  shift 2

  resolve_compose_bin

  _out=()
  if [[ -n "$COMPOSE_BIN" ]]; then
    _out=(
      "$COMPOSE_BIN"
      --env-file "$COMPOSE_ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  elif (( DOCKER_BIN_OVERRIDDEN == 0 )) && command -v docker-compose >/dev/null 2>&1; then
    _out=(
      docker-compose
      --env-file "$COMPOSE_ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  else
    _out=(
      "$DOCKER_BIN"
      compose
      --env-file "$COMPOSE_ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  fi

  _out+=("$@")
}

resolve_reauth_command() {
  local candidate=$1
  local resolved_dir resolved_path

  [[ -n "$candidate" ]] || return 1
  [[ "$candidate" == /* ]] || die "DCE_REAUTH_COMMAND must be an absolute path to an executable script under the repository."

  resolved_dir=$(cd "$(dirname "$candidate")" && pwd -P)
  resolved_path="$resolved_dir/$(basename "$candidate")"
  [[ -f "$resolved_path" ]] || die "DCE_REAUTH_COMMAND does not exist: $candidate"
  [[ -x "$resolved_path" ]] || die "DCE_REAUTH_COMMAND is not executable: $candidate"
  case "$resolved_path" in
    "$REPO_ROOT"/*) ;;
    *) die "DCE_REAUTH_COMMAND must be a script inside the repository root." ;;
  esac

  printf '%s\n' "$resolved_path"
}

resolve_host_config_path() {
  local -a args=("$@")
  local i=0 cfg="$REPO_ROOT/config/scrape-targets.json"

  while (( i < ${#args[@]} )); do
    if [[ "${args[i]}" == "--config" ]]; then
      cfg="${args[i + 1]:-}"
      case "$cfg" in
        /config/*)
          cfg="$REPO_ROOT/config/scrape-targets.json"
          ;;
        ./*)
          cfg="$REPO_ROOT/${cfg#./}"
          ;;
        /*) ;;
        *)
          cfg="$REPO_ROOT/$cfg"
          ;;
      esac
      break
    fi
    i=$((i + 1))
  done

  printf '%s\n' "$cfg"
}

run_disk_preflight_if_enabled() {
  local -a args=("$@")
  local cfg

  if [[ "${DCE_SKIP_DISK_CHECK:-0}" == 1 ]]; then
    return 0
  fi
  if [[ ! -x "$VERIFY_READY" ]]; then
    return 0
  fi

  cfg=$(resolve_host_config_path "${args[@]}")
  "$VERIFY_READY" --disk-only --config "$cfg"
}

is_discord_auth_failure() {
  local output_file=$1
  grep -Eqi \
    "Authentication token is invalid|Request to 'channels/.+' failed: forbidden|failed authenticated preflight|401|403" \
    "$output_file"
}

try_interactive_reauth() {
  local reauth_script

  [[ -n "$REAUTH_COMMAND" ]] || return 1
  [[ -t 0 && -t 1 ]] || return 1
  reauth_script=$(resolve_reauth_command "$REAUTH_COMMAND")
  printf 'Auth failed; running DCE_REAUTH_COMMAND...\n' >&2
  "$reauth_script"
}

run_subcommand_with_retry() {
  local subcommand=$1
  shift
  local -a run_args=()
  local output_file

  ensure_token_present
  output_file=$(mktemp "${TMPDIR:-/tmp}/dce-host-run.XXXXXX.log")

  compose_run_args run_args "$subcommand" "$@"
  if "${run_args[@]}" >"$output_file" 2>&1; then
    cat "$output_file"
    rm -f "$output_file"
    return 0
  fi

  cat "$output_file" >&2

  if ! is_discord_auth_failure "$output_file"; then
    rm -f "$output_file"
    die "Container run failed for '$subcommand' with a non-auth error."
  fi

  printf 'Detected Discord auth failure. Refreshing token and retrying once...\n' >&2
  load_token_from_file || true
  if [[ -z "${DISCORD_TOKEN:-}" ]]; then
    load_token_from_discover_script || true
  fi
  rm -f "$COMPOSE_ENV_TEMP"
  COMPOSE_ENV_TEMP=""
  write_compose_env_temp
  COMPOSE_ENV_FILE="$COMPOSE_ENV_TEMP"
  try_interactive_reauth || true
  ensure_token_present
  compose_run_args run_args "$subcommand" "$@"

  if "${run_args[@]}" >"$output_file" 2>&1; then
    cat "$output_file"
    rm -f "$output_file"
    return 0
  fi

  cat "$output_file" >&2
  rm -f "$output_file"
  die "Container run failed for '$subcommand' after one auth refresh retry."
}

main() {
  local -a passthrough_args=()
  local subcommand=""

  trap cleanup_compose_env EXIT

  while (($#)); do
    case "$1" in
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
        shift 2
        ;;
      --compose-file)
        [[ $# -ge 2 ]] || die "Missing value for --compose-file."
        COMPOSE_FILE=$2
        shift 2
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      preflight|scrape)
        if [[ -n "$subcommand" ]]; then
          passthrough_args+=("$1")
        else
          subcommand=$1
        fi
        shift
        ;;
      *)
        if [[ -z "$subcommand" ]]; then
          die "Unsupported subcommand '$1'. Use 'preflight' or 'scrape'."
        fi
        passthrough_args+=("$1")
        shift
        ;;
    esac
  done

  [[ -n "$subcommand" ]] || {
    usage
    exit 1
  }

  require_program grep
  if [[ -n "$COMPOSE_BIN" ]]; then
    require_program "$COMPOSE_BIN"
  elif (( DOCKER_BIN_OVERRIDDEN == 0 )) && command -v docker-compose >/dev/null 2>&1; then
    :
  else
    require_program "$DOCKER_BIN"
  fi

  [[ -f "$COMPOSE_FILE" ]] || die "Missing compose file: $COMPOSE_FILE"
  prepare_compose_env
  REAUTH_COMMAND="${DCE_REAUTH_COMMAND:-}"
  run_disk_preflight_if_enabled "${passthrough_args[@]}"

  local host_config host_targets=() arg_idx=0
  host_config=$(resolve_host_config_path "${passthrough_args[@]}")
  while (( arg_idx < ${#passthrough_args[@]} )); do
    if [[ "${passthrough_args[arg_idx]}" == "--target" ]]; then
      host_targets+=("${passthrough_args[arg_idx + 1]:-}")
      arg_idx=$((arg_idx + 2))
      continue
    fi
    arg_idx=$((arg_idx + 1))
  done
  print_scrape_config_plan "$host_config" "Host $subcommand" "${host_targets[@]}"

  case "$subcommand" in
    preflight|scrape)
      run_subcommand_with_retry "$subcommand" "${passthrough_args[@]}"
      ;;
  esac
}

main "$@"
