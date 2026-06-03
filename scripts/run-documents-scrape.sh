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
# shellcheck source=lib/scrape-run-plan.sh
source "$SCRIPT_DIR/lib/scrape-run-plan.sh"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--dry-run] [--salvage-only] [--target NAME] [--config PATH]

End-to-end Documents scrape workflow:
  1. Verify enabled targets have seeded archives under ~/Documents/<server>/
  2. Bootstrap scrape.env when DISCORD_TOKEN is exported
  3. Preflight against Discord (skipped with --dry-run or --salvage-only)
  4. Incremental scrape (append-only merges into existing JSON files)

Options:
  --dry-run       Verify archives only; do not call Discord
  --salvage-only  Merge quiescent stale .dce-temp exports only (no Discord export)
  --target NAME Limit preflight/scrape to one configured target
  --channel ID  With exactly one --target, limit scrape to channel ID (repeatable)
  --config PATH Scrape target config (default: config/scrape-targets.json)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  local dry_run=0
  local salvage_only=0
  local target=""
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
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        passthrough+=(--config "$2")
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

  "$VERIFY_SCRIPT" --config "$CONFIG_PATH"

  local -a plan_targets=()
  if [[ -n "$target" ]]; then
    plan_targets=("$target")
  fi
  print_scrape_config_plan "$CONFIG_PATH" "Documents scrape" "${plan_targets[@]}"

  if (( dry_run == 1 )); then
    printf 'Dry run complete: archive paths verified. Export DISCORD_TOKEN or create a token file, then rerun without --dry-run.\n'
    exit 0
  fi

  "$VERIFY_READY" --disk-only --config "$CONFIG_PATH"

  if (( salvage_only == 1 )); then
    local -a salvage_args=(--config "$CONFIG_PATH")
    local skip_next=0 arg
    for arg in "${passthrough[@]}"; do
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
    exit 0
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

  "$HOST_RUNNER" preflight "${container_args[@]}"
  "$HOST_RUNNER" scrape "${container_args[@]}"
}

main "$@"
