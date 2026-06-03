#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"

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

resolve_scrape_lock_file() {
  local config_path=$1

  if [[ -n "${DCE_SCRAPE_LOCK_FILE:-}" ]]; then
    printf '%s\n' "$DCE_SCRAPE_LOCK_FILE"
    return 0
  fi

  local archive_root=""
  if [[ -f "$config_path" ]]; then
    archive_root=$(jq -r '.archive_root // empty' "$config_path" 2>/dev/null) || true
  fi
  if [[ -n "$archive_root" && "$archive_root" != null ]]; then
    printf '%s/.dce-scrape.lock\n' "$archive_root"
  else
    printf '%s/.dce-scrape.lock\n' "$REPO_ROOT"
  fi
}

read_meta_field() {
  local meta_file=$1 field=$2
  grep -E "^${field}=" "$meta_file" 2>/dev/null | head -1 | cut -d= -f2- || true
}

format_holder_line() {
  local meta_file=$1
  local pid="" started="" cmd="" holder_state=""

  [[ -f "$meta_file" ]] || return 0
  pid=$(read_meta_field "$meta_file" pid)
  started=$(read_meta_field "$meta_file" started)
  cmd=$(read_meta_field "$meta_file" cmd)
  [[ -n "$pid" ]] || return 0

  if kill -0 "$pid" 2>/dev/null; then
    holder_state="running"
  else
    holder_state="not running"
  fi
  printf 'holder: pid %s (%s, started %s)\n' "$pid" "$holder_state" "${started:-unknown}"
  [[ -n "$cmd" ]] && printf 'cmd: %s\n' "$cmd"
}

lock_is_held() {
  local lock_file=$1

  command -v flock >/dev/null 2>&1 || return 1
  exec {lock_probe_fd}>>"$lock_file"
  if flock -n "$lock_probe_fd"; then
    flock -u "$lock_probe_fd" 2>/dev/null || true
    exec {lock_probe_fd}>&-
    return 1
  fi
  exec {lock_probe_fd}>&-
  return 0
}

reclaim_stale_lock() {
  local lock_file=$1 meta_file=$2

  if lock_is_held "$lock_file"; then
    die "Cannot reclaim: scrape lock is actively held."
  fi

  if [[ -f "$meta_file" ]]; then
    local pid
    pid=$(read_meta_field "$meta_file" pid)
    if [[ -n "$pid" ]] && kill -0 "$pid" 2>/dev/null; then
      die "Cannot reclaim: holder pid $pid is still running."
    fi
    rm -f "$meta_file"
    printf 'removed stale lock meta: %s\n' "$meta_file"
  fi

  if [[ -e "$lock_file" ]] && ! lock_is_held "$lock_file"; then
    rm -f "$lock_file"
    printf 'removed unheld lock file: %s\n' "$lock_file"
  fi
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

  local lock_file meta_file
  lock_file=$(resolve_scrape_lock_file "$CONFIG_PATH")
  meta_file="${lock_file}.meta"

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
    format_holder_line "$meta_file"
    exit 0
  fi

  if lock_is_held "$lock_file"; then
    printf 'state: held (active scrape)\n'
    format_holder_line "$meta_file"
    exit 1
  fi

  if [[ -f "$meta_file" ]]; then
    local pid
    pid=$(read_meta_field "$meta_file" pid)
    if [[ -n "$pid" ]] && ! kill -0 "$pid" 2>/dev/null; then
      printf 'state: stale (reclaimable; holder pid %s is not running)\n' "$pid"
      format_holder_line "$meta_file"
      if (( reclaim )); then
        reclaim_stale_lock "$lock_file" "$meta_file"
        printf 'state: free (stale lock reclaimed)\n'
      fi
      exit 0
    fi
  fi

  if (( reclaim )); then
    if [[ -e "$lock_file" ]] && ! lock_is_held "$lock_file"; then
      reclaim_stale_lock "$lock_file" "$meta_file"
      printf 'state: free (orphan lock reclaimed)\n'
      exit 0
    fi
    printf 'state: free (nothing to reclaim)\n'
    exit 0
  fi

  printf 'state: free (lock file present but not held)\n'
  format_holder_line "$meta_file"
  exit 0
}

main "$@"
