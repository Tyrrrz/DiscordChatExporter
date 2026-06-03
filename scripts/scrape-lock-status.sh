#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
# shellcheck source=lib/scrape-lock.sh
source "$SCRIPT_DIR/lib/scrape-lock.sh"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--config PATH] [--reclaim-stale]

Report scrape serialization lock state for the configured archive root.
Uses the same lock path rules as run-discord-scrape-host.sh.

  --reclaim-stale  Remove stale .meta and unheld lock file when holder pid is dead

Exit codes:
  0  Safe to scrape (no lock, unheld lock file, or stale reclaimable holder)
  1  Another scrape is actively holding the lock
  2  Configuration or usage error
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 2
}

main() {
  local reclaim=0
  while (($#)); do
    case "$1" in
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --reclaim-stale)
        reclaim=1
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

  command -v jq >/dev/null 2>&1 || die "Required command 'jq' is missing."
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  local lock_file meta_file reclaim_status
  lock_file=$(resolve_scrape_lock_file "$CONFIG_PATH" "$REPO_ROOT")
  meta_file=$(scrape_lock_meta_path "$lock_file")

  printf 'Scrape lock status\n'
  printf '==================\n'
  printf 'config: %s\n' "$CONFIG_PATH"
  printf 'lock: %s\n' "$lock_file"

  if [[ ! -e "$lock_file" ]]; then
    printf 'state: free (no lock file)\n'
    exit 0
  fi

  if ! command -v flock >/dev/null 2>&1; then
    printf 'state: unknown (flock unavailable; lock file exists)\n'
    scrape_lock_format_holder_lines "$meta_file"
    exit 0
  fi

  if scrape_lock_is_held "$lock_file"; then
    printf 'state: held (active scrape)\n'
    scrape_lock_format_holder_lines "$meta_file"
    exit 1
  fi

  if [[ -f "$meta_file" ]]; then
    local pid
    pid=$(read_scrape_lock_meta_field "$meta_file" pid)
    if [[ -n "$pid" ]] && ! kill -0 "$pid" 2>/dev/null; then
      printf 'state: stale (reclaimable; holder pid %s is not running)\n' "$pid"
      scrape_lock_format_holder_lines "$meta_file"
      if (( reclaim )); then
        scrape_lock_reclaim_stale_files "$lock_file" "$meta_file" || die "Cannot reclaim stale scrape lock."
        printf 'state: free (stale lock reclaimed)\n'
      fi
      exit 0
    fi
  fi

  if (( reclaim )); then
    if [[ -e "$lock_file" ]] && ! scrape_lock_is_held "$lock_file"; then
      reclaim_status=0
      scrape_lock_reclaim_stale_files "$lock_file" "$meta_file" || reclaim_status=$?
      if (( reclaim_status == 2 )); then
        die "Cannot reclaim: scrape lock is actively held."
      elif (( reclaim_status == 3 )); then
        die "Cannot reclaim: lock holder pid is still running."
      fi
      printf 'state: free (orphan lock reclaimed)\n'
      exit 0
    fi
    printf 'state: free (nothing to reclaim)\n'
    exit 0
  fi

  printf 'state: free (lock file present but not held)\n'
  scrape_lock_format_holder_lines "$meta_file"
  exit 0
}

main "$@"
