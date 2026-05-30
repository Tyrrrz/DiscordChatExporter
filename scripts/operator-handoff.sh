#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
VERIFY_READY="$REPO_ROOT/scripts/verify-operator-ready.sh"
DOCUMENTS_SCRAPE="$REPO_ROOT/scripts/run-documents-scrape.sh"
SKIP_DF=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--config PATH] [--skip-df]

Run operator handoff checks before cron install or a full scrape:
  1. Free-space summary (archive_root + repo)
  2. verify-operator-ready (jq, compose, auth, archives)
  3. run-documents-scrape --dry-run (archive paths only)

Environment:
  DCE_MIN_FREE_MB   Minimum MiB free (default 1024 in verify-operator-ready)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

print_disk_summary() {
  local archive_root path

  require_command jq
  archive_root=$(jq -r '.archive_root // empty' "$CONFIG_PATH")
  [[ -n "$archive_root" && "$archive_root" != null ]] || die "Config is missing archive_root."

  printf 'Disk summary\n'
  printf '============\n'
  for path in "$archive_root" "$REPO_ROOT"; do
    [[ -e "$path" ]] || continue
    df -hP "$path" | awk 'NR==1 || NR==2 {print}'
    printf '\n'
  done
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

main() {
  while (($#)); do
    case "$1" in
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --skip-df)
        SKIP_DF=1
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

  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  printf 'Operator handoff\n'
  printf '================\n'
  printf 'config: %s\n\n' "$CONFIG_PATH"
  print_scrape_config_plan "$CONFIG_PATH" "Operator handoff"
  printf '\n'

  if (( SKIP_DF == 0 )); then
    print_disk_summary
  fi

  "$VERIFY_READY" --config "$CONFIG_PATH"
  "$DOCUMENTS_SCRAPE" --dry-run --config "$CONFIG_PATH"

  printf '\nHandoff complete. Safe to run:\n'
  printf '  ./scripts/run-documents-scrape.sh\n'
  printf '  ./scripts/setup-cron.sh --dry-run\n'
}

main "$@"
