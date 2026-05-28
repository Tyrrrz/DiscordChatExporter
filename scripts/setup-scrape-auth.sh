#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
EXAMPLE_FILE="$REPO_ROOT/scrape.env.example"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--env-file PATH] [--force]

Create scrape.env for recurring Discord scrapes.

Reads credentials from the environment (never prompts for a token in the terminal):
  DISCORD_TOKEN           Write directly into scrape.env
  DISCORD_TOKEN_FILE      Write a pointer to an existing token file

If scrape.env already exists and --force is not set, the script exits without changes.

Examples:
  export DISCORD_TOKEN="your-token"
  $(basename "$0")

  export DISCORD_TOKEN_FILE="\$HOME/.config/discord-token"
  $(basename "$0")
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  local force=0

  while (($#)); do
    case "$1" in
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
        shift 2
        ;;
      --force)
        force=1
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

  [[ -f "$EXAMPLE_FILE" ]] || die "Missing example env file: $EXAMPLE_FILE"

  if [[ -f "$ENV_FILE" && "$force" -eq 0 ]]; then
    printf 'scrape.env already exists at %s (use --force to overwrite).\n' "$ENV_FILE"
    exit 0
  fi

  if [[ -z "${DISCORD_TOKEN:-}" && -z "${DISCORD_TOKEN_FILE:-}" ]]; then
    die "Set DISCORD_TOKEN or DISCORD_TOKEN_FILE in the environment, then rerun this script."
  fi

  if [[ -n "${DISCORD_TOKEN_FILE:-}" && ! -f "$DISCORD_TOKEN_FILE" ]]; then
    die "DISCORD_TOKEN_FILE does not exist: $DISCORD_TOKEN_FILE"
  fi

  local tmp_file
  tmp_file=$(mktemp "${TMPDIR:-/tmp}/scrape.env.XXXXXX")
  while IFS= read -r line || [[ -n "$line" ]]; do
    case "$line" in
      DISCORD_TOKEN=*)
        if [[ -n "${DISCORD_TOKEN:-}" ]]; then
          printf 'DISCORD_TOKEN=%s\n' "$DISCORD_TOKEN"
        else
          printf '%s\n' "$line"
        fi
        ;;
      DISCORD_TOKEN_FILE=*)
        if [[ -n "${DISCORD_TOKEN_FILE:-}" ]]; then
          printf 'DISCORD_TOKEN_FILE=%s\n' "$DISCORD_TOKEN_FILE"
        else
          printf '%s\n' "$line"
        fi
        ;;
      *)
        printf '%s\n' "$line"
        ;;
    esac
  done <"$EXAMPLE_FILE" >"$tmp_file"
  mv "$tmp_file" "$ENV_FILE"

  chmod 600 "$ENV_FILE"
  printf 'Created %s (mode 600).\n' "$ENV_FILE"
  printf 'Next: ./scripts/verify-documents-archives.sh && ./scripts/run-discord-scrape-host.sh preflight\n'
}

main "$@"
