#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
DISCOVER="$REPO_ROOT/scripts/discover-discord-token.sh"
SETUP_AUTH="$REPO_ROOT/scripts/setup-scrape-auth.sh"
FORCE=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--env-file PATH] [--force]

Write scrape.env from the DiscordChatExporter GUI Settings.dat or other
discovered token sources (see scripts/discover-discord-token.sh).

Options:
  --env-file PATH  Output env file (default: scrape.env)
  --force          Overwrite existing scrape.env
  --help           Show this help text
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  while (($#)); do
    case "$1" in
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
        shift 2
        ;;
      --force)
        FORCE=1
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

  [[ -x "$DISCOVER" ]] || die "Missing discover script: $DISCOVER"

  if [[ -f "$ENV_FILE" && $FORCE -eq 0 ]]; then
    die "$ENV_FILE already exists. Use --force to replace it from the GUI token."
  fi

  local token
  token=$("$DISCOVER") || die "Could not discover a Discord token. Set DISCORDCHATEXPORTER_SETTINGS_PATH or export DISCORD_TOKEN."

  DISCORD_TOKEN=$token "$SETUP_AUTH" --env-file "$ENV_FILE" --force
  chmod 600 "$ENV_FILE" 2>/dev/null || true
  printf 'Updated %s from discovered GUI/client token.\n' "$ENV_FILE"
}

main "$@"
