#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
SOURCE_DOC="$REPO_ROOT/docs/gui-zip-recurring-scrape-bridge.md"
DEFAULT_DEST="$REPO_ROOT/../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md"
DEST_PATH="${DCE_GUI_BRIDGE_DEST:-$DEFAULT_DEST}"
DRY_RUN=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--dest PATH] [--dry-run]

Copy the versioned GUI zip quick-start from docs/gui-zip-recurring-scrape-bridge.md
to the sibling GUI folder (default: ../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md).
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  while (($#)); do
    case "$1" in
      --dest)
        [[ $# -ge 2 ]] || die "Missing value for --dest."
        DEST_PATH=$2
        shift 2
        ;;
      --dry-run)
        DRY_RUN=1
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

  [[ -f "$SOURCE_DOC" ]] || die "Missing source doc: $SOURCE_DOC"
  local dest_dir
  dest_dir=$(dirname "$DEST_PATH")
  [[ -d "$dest_dir" ]] || die "Destination directory does not exist: $dest_dir (create it or pass --dest)"

  if (( DRY_RUN == 1 )); then
    printf 'Would copy:\n  %s\n  -> %s\n' "$SOURCE_DOC" "$DEST_PATH"
    exit 0
  fi

  cp -f "$SOURCE_DOC" "$DEST_PATH"
  printf 'Synced GUI bridge doc to %s\n' "$DEST_PATH"
}

main "$@"
