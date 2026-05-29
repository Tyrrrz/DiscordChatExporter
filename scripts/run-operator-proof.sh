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

TARGET="eod_discord"
SYNC_GUI_FLAG=0
DRY_RUN=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--target NAME] [--config PATH] [--sync-gui] [--dry-run]

End-to-end operator proof for one target:
  operator-handoff → incremental scrape → prove-incremental-append

Logs append to logs/operator-proof-<timestamp>.log
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
      --help|-h)
        usage
        exit 0
        ;;
      *)
        die "Unknown option: $1"
        ;;
    esac
  done

  mkdir -p "$LOG_DIR"
  local log_file
  log_file="$LOG_DIR/operator-proof-$(date -u +%Y%m%dT%H%M%SZ).log"

  {
    printf 'Operator proof for target %s\n' "$TARGET"
    printf 'config: %s\n' "$CONFIG_PATH"
    printf 'started: %s\n\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"

    if (( SYNC_GUI_FLAG == 1 )); then
      [[ -x "$SYNC_GUI" ]] || die "Missing sync-token-from-gui.sh"
      "$SYNC_GUI" --force
    fi

    if (( DRY_RUN == 1 )); then
      "$HANDOFF" --config "$CONFIG_PATH"
      printf '\nDry run complete (no Discord scrape).\n'
      exit 0
    fi

    "$HANDOFF" --config "$CONFIG_PATH"
    "$DOCUMENTS" --config "$CONFIG_PATH" --target "$TARGET"
    "$PROVE" --config "$CONFIG_PATH" --target "$TARGET"

    printf '\nOperator proof succeeded for %s\n' "$TARGET"
  } 2>&1 | tee "$log_file"

  printf 'Log: %s\n' "$log_file"
}

main "$@"
