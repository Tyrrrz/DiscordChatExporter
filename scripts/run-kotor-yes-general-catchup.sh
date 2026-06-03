#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
LOG_DIR="${DCE_LOG_DIR:-$REPO_ROOT/logs}"
LOG_FILE="${DCE_KOTOR_LOG_FILE:-$LOG_DIR/kotor-yes-general.log}"
DOCUMENTS="$REPO_ROOT/scripts/run-documents-scrape.sh"
VALIDATION="$REPO_ROOT/scripts/run-operator-validation.sh"
PROVE="$REPO_ROOT/scripts/prove-incremental-append.sh"
PRINT_SUMMARY="$REPO_ROOT/scripts/print-scrape-summary.sh"

TARGET=KotOR_discord_msgs
CHANNEL=221726893064454144

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [options]

Focused operator entry for KotOR yes_general (channel $CHANNEL):
  salvage-before-scrape → incremental documents scrape → optional summary hint

Options:
  --dry-run       Archive verify only (no Discord)
  --salvage-only  Merge stale .dce-temp exports only
  --validation    Run operator-validation (salvage-before-scrape + audit)
  --prove         Run prove-incremental-append for this channel only
  --log-file PATH Log for scrape/validation (default: logs/kotor-yes-general.log)
  --config PATH   Targets JSON (default: config/scrape-targets.json)
  --help          Show this help text

Live scrape (default):
  $(basename "$0")

After a live run, inspect:
  ./scripts/print-scrape-summary.sh logs/kotor-yes-general.summary.json
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  local dry_run=0 salvage_only=0 validation=0 prove=0

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
      --validation)
        validation=1
        shift
        ;;
      --prove)
        prove=1
        shift
        ;;
      --log-file)
        [[ $# -ge 2 ]] || die "Missing value for --log-file."
        LOG_FILE=$2
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
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

  local modes=0
  (( dry_run == 1 )) && modes=$((modes + 1))
  (( salvage_only == 1 )) && modes=$((modes + 1))
  (( validation == 1 )) && modes=$((modes + 1))
  (( prove == 1 )) && modes=$((modes + 1))
  (( modes > 1 )) && die "Use only one of --dry-run, --salvage-only, --validation, or --prove."

  local -a common=(--config "$CONFIG_PATH" --target "$TARGET" --channel "$CHANNEL")

  if (( prove == 1 )); then
    exec "$PROVE" "${common[@]}"
  fi

  if (( validation == 1 )); then
    exec "$VALIDATION" --salvage-before-scrape "${common[@]}" --log-file "$LOG_FILE"
  fi

  if (( dry_run == 1 )); then
    exec "$DOCUMENTS" --dry-run "${common[@]}"
  fi

  if (( salvage_only == 1 )); then
    exec "$DOCUMENTS" --salvage-only "${common[@]}"
  fi

  printf 'KotOR yes_general catch-up: target=%s channel=%s\n' "$TARGET" "$CHANNEL"
  printf 'Log file: %s\n' "$LOG_FILE"
  "$DOCUMENTS" --salvage-before-scrape "${common[@]}" --log-file "$LOG_FILE"
  local st=$?
  printf 'Inspect summary: %s %s.summary.json\n' "$PRINT_SUMMARY" "${LOG_FILE%.log}"
  exit "$st"
}

main "$@"
