#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
TOOL_DIR="$REPO_ROOT/scripts/tools/ReadDceGuiToken"
TOOL_BIN="$TOOL_DIR/bin/Release/net10.0/ReadDceGuiToken"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [SETTINGS.dat]

Print the Discord token stored by DiscordChatExporter GUI (decrypts enc: values).
Token is written to stdout only — redirect to a mode-600 file if persisting.

Environment:
  DISCORDCHATEXPORTER_SETTINGS_PATH  Path to Settings.dat or its parent directory
  DCE_ENCRYPTION_SALT                Override encryption salt (default: HimalayanPinkSalt)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

ensure_tool_built() {
  if [[ -x "$TOOL_BIN" ]]; then
    return 0
  fi

  command -v dotnet >/dev/null 2>&1 || die "dotnet SDK is required to decrypt GUI settings."

  dotnet build "$TOOL_DIR" -c Release -v q >/dev/null
  [[ -x "$TOOL_BIN" ]] || die "Failed to build ReadDceGuiToken at $TOOL_BIN"
}

main() {
  if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
    usage
    exit 0
  fi

  ensure_tool_built
  "$TOOL_BIN" "${1:-}"
}

main "$@"
