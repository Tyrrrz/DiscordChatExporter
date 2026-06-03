#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
HANDOFF="$REPO_ROOT/scripts/operator-handoff.sh"
DOCUMENTS="$REPO_ROOT/scripts/run-documents-scrape.sh"
PROVE="$REPO_ROOT/scripts/prove-incremental-append.sh"
SYNC_GUI="$REPO_ROOT/scripts/sync-token-from-gui.sh"
LOG_DIR="$REPO_ROOT/logs"
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"

TARGET=""
SYNC_GUI_FLAG=0
DRY_RUN=0
SALVAGE_BEFORE=0
SALVAGE_ONLY=0
CHANNEL_ARGS=()
LOG_FILE=""

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--target NAME] [--channel ID] [--config PATH] [--sync-gui] [--dry-run] [--salvage-only] [--salvage-before-scrape]

End-to-end operator proof:
  operator-handoff → [optional salvage] → incremental scrape → prove-incremental-append

When --target is omitted, all enabled targets in the config are processed.

  --channel ID  With exactly one --target, limit scrape/prove to channel ID (repeatable)
  --salvage-only  Handoff + merge stale .dce-temp exports only (no Discord scrape or prove)
  --salvage-before-scrape  Merge stale .dce-temp exports before incremental scrape
  --log-file PATH          Append output to this file (default: logs/operator-proof-UTC.log)

Logs append to logs/operator-proof-<timestamp>.log (or --log-file). When scraping, also writes
<log-basename>.summary.json unless DCE_RUN_SUMMARY_FILE is already set.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  while (($#)); do
    case "$1" in
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        TARGET=$2
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --sync-gui)
        SYNC_GUI_FLAG=1
        shift
        ;;
      --dry-run)
        DRY_RUN=1
        shift
        ;;
      --salvage-before-scrape)
        SALVAGE_BEFORE=1
        shift
        ;;
      --salvage-only)
        SALVAGE_ONLY=1
        shift
        ;;
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        CHANNEL_ARGS+=(--channel "$2")
        shift 2
        ;;
      --log-file)
        [[ $# -ge 2 ]] || die "Missing value for --log-file."
        LOG_FILE=$2
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

  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  if (( SALVAGE_ONLY == 1 && DRY_RUN == 1 )); then
    die "--salvage-only cannot be combined with --dry-run."
  fi
  if (( SALVAGE_ONLY == 1 && SALVAGE_BEFORE == 1 )); then
    die "--salvage-only and --salvage-before-scrape are mutually exclusive."
  fi

  local -a targets=()
  if [[ -n "$TARGET" ]]; then
    targets=("$TARGET")
  else
    mapfile -t targets < <(enabled_target_names "$CONFIG_PATH")
    ((${#targets[@]} > 0)) || die "No enabled targets in $CONFIG_PATH"
  fi

  mkdir -p "$LOG_DIR"
  local log_file
  if [[ -n "$LOG_FILE" ]]; then
    log_file="$LOG_FILE"
  else
    log_file="$LOG_DIR/operator-proof-$(date -u +%Y%m%dT%H%M%SZ).log"
  fi

  local export_json_summary=0
  if (( DRY_RUN == 0 && SALVAGE_ONLY == 0 )); then
    export_json_summary=1
    export DCE_RUN_SUMMARY_JSON=1
    if [[ -z "${DCE_RUN_SUMMARY_FILE:-}" ]]; then
      export DCE_RUN_SUMMARY_FILE="${log_file%.log}.summary.json"
    fi
  fi

  local failed=0 succeeded=0 name

  {
    if [[ -n "$TARGET" ]]; then
      printf 'Operator proof for target %s\n' "$TARGET"
    else
      printf 'Operator proof for %s enabled target(s)\n' "${#targets[@]}"
    fi
    printf 'config: %s\n' "$CONFIG_PATH"
    print_scrape_config_plan "$CONFIG_PATH" "Operator proof" "${targets[@]}"
    if (( export_json_summary )); then
      printf 'JSON summary file: %s\n' "${DCE_RUN_SUMMARY_FILE:-}"
    fi
    printf 'started: %s\n\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"

    if (( SYNC_GUI_FLAG == 1 )); then
      [[ -x "$SYNC_GUI" ]] || die "Missing sync-token-from-gui.sh"
      "$SYNC_GUI" --force
    fi

    local -a handoff_args=(--config "$CONFIG_PATH")
    [[ -n "$TARGET" ]] && handoff_args+=(--target "$TARGET")
    handoff_args+=("${CHANNEL_ARGS[@]}")
    (( SALVAGE_ONLY )) && handoff_args+=(--salvage-only)
    "$HANDOFF" "${handoff_args[@]}"

    if (( DRY_RUN == 1 )); then
      printf '\nDry run complete (no Discord scrape).\n'
      exit 0
    fi
    if (( SALVAGE_ONLY == 1 )); then
      printf '\nSalvage-only proof complete (no Discord scrape or append proof).\n'
      exit 0
    fi

    for name in "${targets[@]}"; do
      printf '\n--- Target: %s ---\n' "$name"
      local -a scrape_args=(--config "$CONFIG_PATH" --target "$name")
      scrape_args+=("${CHANNEL_ARGS[@]}")
      if (( SALVAGE_BEFORE )); then
        if ! "$DOCUMENTS" "${scrape_args[@]}" --salvage-only; then
          failed=$((failed + 1))
          printf 'Operator proof FAILED for %s (salvage-before)\n' "$name" >&2
          continue
        fi
      fi
      if "$DOCUMENTS" "${scrape_args[@]}" && "$PROVE" "${scrape_args[@]}"; then
        succeeded=$((succeeded + 1))
        printf 'Operator proof passed for %s\n' "$name"
      else
        failed=$((failed + 1))
        printf 'Operator proof FAILED for %s\n' "$name" >&2
      fi
    done

    printf '\nOperator proof summary: %s succeeded, %s failed (of %s)\n' \
      "$succeeded" "$failed" "${#targets[@]}"
    (( failed == 0 )) || exit 1
  } 2>&1 | tee "$log_file"

  if (( export_json_summary )) && [[ -n "${DCE_RUN_SUMMARY_FILE:-}" ]]; then
    # shellcheck source=lib/scrape-summary-json.sh
    source "$SCRIPT_DIR/lib/scrape-summary-json.sh"
    if recover_json_summary_if_missing "$log_file" "$DCE_RUN_SUMMARY_FILE"; then
      printf 'JSON summary recovered from log: %s\n' "$DCE_RUN_SUMMARY_FILE"
    fi
  fi

  printf 'Log: %s\n' "$log_file"
}

main "$@"
