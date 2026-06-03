#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
HOST_RUNNER="$REPO_ROOT/scripts/run-discord-scrape-host.sh"
VERIFY_ARCHIVES="$REPO_ROOT/scripts/verify-documents-archives.sh"
DISCOVER="$REPO_ROOT/scripts/discover-discord-token.sh"
PREFLIGHT_TARGET=""
DISK_ONLY=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--config PATH] [--disk-only] [--preflight TARGET]

Check host prerequisites for recurring scrape:
  jq, container compose, Discord auth, valid config, seeded archives.
With --disk-only, only validate config JSON and archive-root free space (DCE_MIN_FREE_MB).
With --preflight TARGET, also run Discord preflight for one target.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

resolve_compose() {
  if [[ -n "${DCE_COMPOSE_BIN:-}" ]]; then
    printf 'compose: %s\n' "$DCE_COMPOSE_BIN"
    return 0
  fi
  if command -v podman-compose >/dev/null 2>&1 && podman info >/dev/null 2>&1; then
    printf 'compose: podman-compose\n'
    return 0
  fi
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    printf 'compose: docker compose\n'
    return 0
  fi
  if command -v docker-compose >/dev/null 2>&1; then
    printf 'compose: docker-compose\n'
    return 0
  fi
  if command -v podman >/dev/null 2>&1 && podman compose version >/dev/null 2>&1; then
    printf 'compose: podman compose\n'
    return 0
  fi
  die "Install Docker or Podman with compose support."
}

require_archive_disk_space() {
  local min_mb=${DCE_MIN_FREE_MB:-1024}
  local archive_root path avail_kb need_kb

  if (( min_mb <= 0 )); then
    printf 'disk: check skipped (DCE_MIN_FREE_MB=%s)\n' "$min_mb"
    return 0
  fi

  archive_root=$(jq -r '.archive_root // empty' "$CONFIG_PATH")
  [[ -n "$archive_root" && "$archive_root" != null ]] || die "Config is missing archive_root."
  need_kb=$((min_mb * 1024))

  for path in "$archive_root" "$REPO_ROOT"; do
    [[ -e "$path" ]] || continue
    avail_kb=$(df -Pk "$path" | awk 'NR==2 {print $4}')
    [[ -n "$avail_kb" && "$avail_kb" =~ ^[0-9]+$ ]] || die "Could not read free space for $path"
    if (( avail_kb < need_kb )); then
      die "Insufficient disk space on $(df -Pk "$path" | awk 'NR==2 {print $6}'): $((avail_kb / 1024)) MiB free, need at least ${min_mb} MiB under archive_root ($archive_root). Free space before scraping."
    fi
    printf 'disk: %s has %s MiB free (need %s MiB)\n' "$(df -Pk "$path" | awk 'NR==2 {print $6}')" "$((avail_kb / 1024))" "$min_mb"
  done
}

check_auth() {
  if [[ -f "$ENV_FILE" ]] && grep -qE '^[[:space:]]*DISCORD_TOKEN=' "$ENV_FILE"; then
    printf 'auth: scrape.env has DISCORD_TOKEN\n'
    return 0
  fi
  if [[ -n "${DISCORD_TOKEN:-}" ]]; then
    printf 'auth: DISCORD_TOKEN exported in environment\n'
    return 0
  fi
  if [[ -x "$DISCOVER" ]]; then
    local token
    token=$("$DISCOVER" 2>/dev/null || true)
    if [[ -n "$token" ]]; then
      printf 'auth: token discoverable (GUI or config paths)\n'
      return 0
    fi
  fi
  die "No Discord token: set scrape.env, export DISCORD_TOKEN, or sync from GUI."
}

print_container_memory() {
  local mem=""

  if [[ -f "$ENV_FILE" ]]; then
    mem=$(grep -E '^[[:space:]]*DCE_CONTAINER_MEMORY=' "$ENV_FILE" 2>/dev/null | tail -1 | cut -d= -f2- | tr -d '\r' || true)
  fi
  if [[ -z "$mem" && -n "${DCE_CONTAINER_MEMORY:-}" ]]; then
    mem="$DCE_CONTAINER_MEMORY"
  fi
  mem=${mem#"${mem%%[![:space:]]*}"}
  mem=${mem%"${mem##*[![:space:]]}"}
  [[ -n "$mem" && "$mem" != "0" ]] || return 0
  printf 'container memory: %s (compose mem_limit)\n' "$mem"
}

print_config_target_memory() {
  local global_mem="" name mem

  if [[ -f "$ENV_FILE" ]]; then
    global_mem=$(grep -E '^[[:space:]]*DCE_CONTAINER_MEMORY=' "$ENV_FILE" 2>/dev/null | tail -1 | cut -d= -f2- | tr -d '\r' || true)
  fi
  if [[ -z "$global_mem" && -n "${DCE_CONTAINER_MEMORY:-}" ]]; then
    global_mem="$DCE_CONTAINER_MEMORY"
  fi
  global_mem=${global_mem#"${global_mem%%[![:space:]]*}"}
  global_mem=${global_mem%"${global_mem##*[![:space:]]}"}
  if [[ -n "$global_mem" && "$global_mem" != "0" ]]; then
    return 0
  fi

  while IFS= read -r name; do
    [[ -n "$name" ]] || continue
    mem=$(target_container_memory "$CONFIG_PATH" "$name")
    [[ -n "$mem" && "$mem" != "null" ]] || continue
    printf 'target memory: %s → %s (single --target scrape)\n' "$name" "$mem"
  done < <(enabled_target_names "$CONFIG_PATH")
}

main() {
  while (($#)); do
    case "$1" in
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --disk-only)
        DISK_ONLY=1
        shift
        ;;
      --preflight)
        [[ $# -ge 2 ]] || die "Missing value for --preflight."
        PREFLIGHT_TARGET=$2
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

  require_command jq
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"
  jq empty "$CONFIG_PATH" >/dev/null 2>&1 || die "Invalid JSON config: $CONFIG_PATH"

  if (( DISK_ONLY == 1 )); then
    require_archive_disk_space
    printf 'disk-only: ok (config %s)\n' "$CONFIG_PATH"
    exit 0
  fi

  printf 'Operator readiness checks\n'
  printf '=========================\n'
  require_archive_disk_space
  resolve_compose
  check_auth
  print_container_memory
  print_config_target_memory
  printf 'config: %s\n\n' "$CONFIG_PATH"

  DCE_PRIMARY_CONFIG="$CONFIG_PATH" "$VERIFY_ARCHIVES" --config "$CONFIG_PATH"

  if [[ -n "$PREFLIGHT_TARGET" ]]; then
    printf '\nRunning preflight for target %s...\n' "$PREFLIGHT_TARGET"
    "$HOST_RUNNER" preflight --config /config/scrape-targets.json --target "$PREFLIGHT_TARGET"
  fi

  printf '\nOperator ready. Next:\n'
  printf '  ./scripts/run-documents-scrape.sh\n'
  printf '  ./scripts/setup-cron.sh --dry-run\n'
}

main "$@"
