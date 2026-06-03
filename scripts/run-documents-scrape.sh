#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_PRIMARY_CONFIG:-$REPO_ROOT/config/scrape-targets.json}"
CONTAINER_CONFIG="${DCE_CONTAINER_CONFIG:-/config/scrape-targets.json}"
HOST_RUNNER="$REPO_ROOT/scripts/run-discord-scrape-host.sh"
DISCOVER_TOKEN="$REPO_ROOT/scripts/discover-discord-token.sh"
VERIFY_SCRIPT="$REPO_ROOT/scripts/verify-documents-archives.sh"
VERIFY_READY="$REPO_ROOT/scripts/verify-operator-ready.sh"
SETUP_AUTH="$REPO_ROOT/scripts/setup-scrape-auth.sh"
LOCK_STATUS="$REPO_ROOT/scripts/scrape-lock-status.sh"
LOG_DIR="${DCE_LOG_DIR:-$REPO_ROOT/logs}"
# shellcheck source=lib/scrape-lock.sh
source "$SCRIPT_DIR/lib/scrape-lock.sh"
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--dry-run] [--salvage-only] [--salvage-before-scrape] [--target NAME] [--config PATH]

End-to-end Documents scrape workflow:
  1. Verify enabled targets have seeded archives under ~/Documents/<server>/
  2. Bootstrap scrape.env when DISCORD_TOKEN is exported
  3. Preflight against Discord (skipped with --dry-run or --salvage-only)
  4. Incremental scrape (append-only merges into existing JSON files)

Options:
  --dry-run       Verify archives only; do not call Discord
  --salvage-only  Merge quiescent stale .dce-temp exports only (no Discord export)
  --salvage-before-scrape  Run salvage-only pass before preflight and incremental scrape
  --target NAME Limit preflight/scrape to one configured target
  --channel ID  With exactly one --target, limit scrape to channel ID (repeatable)
  --guild ID    With exactly one --target, limit scrape to guild ID (repeatable)
  --config PATH Scrape target config (default: config/scrape-targets.json)
  --log-file PATH Append full workflow output to this file (default on live scrape: logs/documents-scrape-UTC.log)
  --summary-file PATH  Machine-readable scrape summary JSON (default: <log-basename>.summary.json on live scrape)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_scrape_lock_free() {
  if ! ensure_scrape_lock_available "$CONFIG_PATH" "$LOCK_STATUS"; then
    die "Scrape lock is held; another scrape may be running. Inspect: $LOCK_STATUS --config $CONFIG_PATH"
  fi
}

run_local_salvage() {
  local -a salvage_args=(--config "$CONFIG_PATH")
  local skip_next=0 arg
  for arg in "$@"; do
    if (( skip_next )); then
      skip_next=0
      continue
    fi
    if [[ "$arg" == "--config" ]]; then
      skip_next=1
      continue
    fi
    salvage_args+=("$arg")
  done
  "$HOST_RUNNER" salvage "${salvage_args[@]}"
}

run_documents_scrape_workflow() {
  local dry_run=$1
  local salvage_only=$2
  local salvage_before=$3
  local target=$4
  local log_file=$5
  local -a passthrough=("${@:6}")

  "$VERIFY_SCRIPT" --config "$CONFIG_PATH"

  local -a plan_targets=()
  if [[ -n "$target" ]]; then
    plan_targets=("$target")
  fi
  print_scrape_config_plan "$CONFIG_PATH" "Documents scrape" "${plan_targets[@]}"

  if (( dry_run == 1 )); then
    printf 'Dry run complete: archive paths verified. Export DISCORD_TOKEN or create a token file, then rerun without --dry-run.\n'
    return 0
  fi

  "$VERIFY_READY" --disk-only --config "$CONFIG_PATH"

  require_scrape_lock_free

  if (( salvage_only == 1 )); then
    run_local_salvage "${passthrough[@]}"
    return 0
  fi

  if (( salvage_before == 1 )); then
    run_local_salvage "${passthrough[@]}"
  fi

  local -a container_args=("${passthrough[@]}")
  local has_config=0 idx=0

  while (( idx < ${#container_args[@]} )); do
    if [[ "${container_args[idx]}" == "--config" ]]; then
      has_config=1
      case "${container_args[idx + 1]:-}" in
        "$CONFIG_PATH"|config/scrape-targets.json|./config/scrape-targets.json)
          container_args[idx + 1]="$CONTAINER_CONFIG"
          ;;
      esac
      break
    fi
    idx=$((idx + 1))
  done

  if (( has_config == 0 )); then
    container_args=(--config "$CONTAINER_CONFIG" "${container_args[@]}")
  fi

  if [[ -n "${DISCORD_TOKEN:-}" || -n "${DISCORD_TOKEN_FILE:-}" ]]; then
    "$SETUP_AUTH" 2>/dev/null || true
  elif [[ -x "$DISCOVER_TOKEN" ]] && "$DISCOVER_TOKEN" >/dev/null 2>&1; then
    "$SETUP_AUTH" 2>/dev/null || true
  fi

  if [[ -n "$log_file" ]]; then
    printf 'Log file: %s\n' "$log_file"
  fi
  printf 'JSON summary file: %s\n' "${DCE_RUN_SUMMARY_FILE:-}"

  "$HOST_RUNNER" preflight "${container_args[@]}"
  "$HOST_RUNNER" scrape "${container_args[@]}"
}

main() {
  local dry_run=0
  local salvage_only=0
  local salvage_before=0
  local target=""
  local summary_file=""
  local log_file=""
  local -a passthrough=()

  while (($#)); do
    case "$1" in
      --dry-run)
        dry_run=1
        shift
        ;;
      --salvage-only)
        salvage_only=1
        shift
        ;;
      --salvage-before-scrape)
        salvage_before=1
        shift
        ;;
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        target=$2
        passthrough+=(--target "$2")
        shift 2
        ;;
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        passthrough+=(--channel "$2")
        shift 2
        ;;
      --guild)
        [[ $# -ge 2 ]] || die "Missing value for --guild."
        passthrough+=(--guild "$2")
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        passthrough+=(--config "$2")
        shift 2
        ;;
      --log-file)
        [[ $# -ge 2 ]] || die "Missing value for --log-file."
        log_file=$2
        shift 2
        ;;
      --summary-file)
        [[ $# -ge 2 ]] || die "Missing value for --summary-file."
        summary_file=$2
        shift 2
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

  local exclusive=0
  (( dry_run == 1 )) && exclusive=$((exclusive + 1))
  (( salvage_only == 1 )) && exclusive=$((exclusive + 1))
  (( salvage_before == 1 )) && exclusive=$((exclusive + 1))
  if (( exclusive > 1 )); then
    die "Use only one of --dry-run, --salvage-only, or --salvage-before-scrape."
  fi

  local export_json_summary=0
  if (( dry_run == 0 && salvage_only == 0 )); then
    export_json_summary=1
    mkdir -p "$LOG_DIR"
    if [[ -z "$log_file" ]]; then
      log_file="$LOG_DIR/documents-scrape-$(date -u +%Y%m%dT%H%M%SZ).log"
    fi
    export DCE_RUN_SUMMARY_JSON=1
    if [[ -z "${DCE_RUN_SUMMARY_FILE:-}" ]]; then
      if [[ -n "$summary_file" ]]; then
        export DCE_RUN_SUMMARY_FILE="$summary_file"
      else
        export DCE_RUN_SUMMARY_FILE="${log_file%.log}.summary.json"
      fi
    fi
  fi

  local pipeline_status=0
  if [[ -n "$log_file" ]]; then
    mkdir -p "$(dirname "$log_file")"
    set -o pipefail
    {
      run_documents_scrape_workflow "$dry_run" "$salvage_only" "$salvage_before" "$target" "$log_file" "${passthrough[@]}"
    } 2>&1 | tee -a "$log_file"
    pipeline_status=${PIPESTATUS[0]}
  else
    run_documents_scrape_workflow "$dry_run" "$salvage_only" "$salvage_before" "$target" "" "${passthrough[@]}"
    pipeline_status=$?
  fi

  if (( export_json_summary )) && [[ -n "${DCE_RUN_SUMMARY_FILE:-}" && -n "$log_file" ]]; then
    # shellcheck source=lib/scrape-summary-json.sh
    source "$SCRIPT_DIR/lib/scrape-summary-json.sh"
    if recover_json_summary_if_missing "$log_file" "$DCE_RUN_SUMMARY_FILE"; then
      printf 'JSON summary recovered from log: %s\n' "$DCE_RUN_SUMMARY_FILE"
    fi
  fi

  if [[ -n "$log_file" ]]; then
    printf 'Log: %s\n' "$log_file"
  fi

  exit "$pipeline_status"
}

main "$@"
