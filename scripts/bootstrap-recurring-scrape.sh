#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
COMPOSE_FILE="${DCE_COMPOSE_FILE:-$REPO_ROOT/docker-compose.yml}"
HOST_RUNNER="$REPO_ROOT/scripts/run-discord-scrape-host.sh"
VERIFY_SCRIPT="$REPO_ROOT/scripts/verify-documents-archives.sh"
SETUP_AUTH="$REPO_ROOT/scripts/setup-scrape-auth.sh"

DRY_RUN=0
SKIP_BUILD=0
TARGETS=()

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [options]

Bootstrap recurring append-only Discord scrapes:
  1. Verify ~/Documents archive folders (config/scrape-targets.json)
  2. Build the source Docker image (unless --skip-build)
  3. Ensure scrape.env exists when DISCORD_TOKEN is exported
  4. Run authenticated preflight (skipped with --dry-run)

Options:
  --dry-run       Verify archives only; do not build or call Discord
  --skip-build    Skip docker compose build
  --target NAME   Limit preflight to one configured target (repeatable)
  --config PATH   Targets JSON (default: config/scrape-targets.json)
  --env-file PATH Compose env file (default: scrape.env)
  --help          Show this help text

Next steps after success:
  ./scripts/run-documents-scrape.sh
  ./scripts/setup-cron.sh --dry-run
  ./scripts/setup-cron.sh
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_program() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

resolve_compose() {
  if [[ -n "${DCE_COMPOSE_BIN:-}" ]]; then
    COMPOSE_BIN=("$DCE_COMPOSE_BIN")
    return 0
  fi
  if command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_BIN=(docker-compose)
    return 0
  fi
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    COMPOSE_BIN=(docker compose)
    return 0
  fi
  if command -v podman >/dev/null 2>&1 && podman compose version >/dev/null 2>&1; then
    COMPOSE_BIN=(podman compose)
    return 0
  fi
  die "Install Docker or Podman with compose support."
}

main() {
  while (($#)); do
    case "$1" in
      --dry-run)
        DRY_RUN=1
        shift
        ;;
      --skip-build)
        SKIP_BUILD=1
        shift
        ;;
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        TARGETS+=("$2")
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
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

  require_program jq
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  "$VERIFY_SCRIPT" --config "$CONFIG_PATH"

  if (( DRY_RUN == 1 )); then
    printf 'Dry run complete: archive paths verified under configured output_dir values.\n'
    printf 'Next: cp scrape.env.example scrape.env, set DISCORD_TOKEN, then rerun without --dry-run.\n'
    exit 0
  fi

  if (( SKIP_BUILD == 0 )); then
    resolve_compose
    (cd "$REPO_ROOT" && "${COMPOSE_BIN[@]}" -f "$COMPOSE_FILE" build)
  fi

  if [[ -n "${DISCORD_TOKEN:-}" || -n "${DISCORD_TOKEN_FILE:-}" ]]; then
    "$SETUP_AUTH" --env-file "$ENV_FILE" 2>/dev/null || true
  fi

  [[ -f "$ENV_FILE" ]] || die "Missing $ENV_FILE. Copy scrape.env.example or export DISCORD_TOKEN and run scripts/setup-scrape-auth.sh."

  local -a preflight_args=("$HOST_RUNNER" --env-file "$ENV_FILE" --compose-file "$COMPOSE_FILE" preflight)
  local target
  for target in "${TARGETS[@]}"; do
    preflight_args+=(--target "$target")
  done

  local preflight_log preflight_status
  preflight_log=$(mktemp "${TMPDIR:-/tmp}/dce-bootstrap-preflight.XXXXXX")
  "${preflight_args[@]}" 2>&1 | tee "$preflight_log"
  preflight_status=${PIPESTATUS[0]}

  if (( preflight_status != 0 )); then
    cat "$preflight_log" >&2
    rm -f "$preflight_log"
    exit "$preflight_status"
  fi

  printf '\nBootstrap complete.\n'
  if grep -q 'inaccessible, but .* seeded archive' "$preflight_log" \
    || grep -qiE 'failed: forbidden|Missing Access' "$preflight_log"; then
    printf '\nToken note: many channels returned forbidden. That usually means a bot token without message-history access.\n'
    printf '  For live incremental downloads, run: %s --force\n' "$REPO_ROOT/scripts/sync-token-from-gui.sh"
    printf '  Or put a user token in %s (see .docs/Token-and-IDs.md).\n' "$ENV_FILE"
    printf '  Append-only archives are still safe: existing JSON is updated in place and never fully re-downloaded.\n'
  fi
  rm -f "$preflight_log"

  printf '  Scrape now:  %s\n' "$REPO_ROOT/scripts/run-documents-scrape.sh"
  printf '  Install cron: %s --dry-run\n' "$REPO_ROOT/scripts/setup-cron.sh"
}

main "$@"
