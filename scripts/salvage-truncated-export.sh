#!/usr/bin/env bash

set -Eeuo pipefail

usage() {
  cat <<EOF
Usage:
  $(basename "$0") PATH [--dry-run]

Repair a truncated DiscordChatExporter JSON export by dropping the incomplete
final message and closing the messages array.

Creates PATH.bak.<timestamp> before modifying PATH.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  local export_path=${1:-}
  local dry_run=0

  [[ -n "$export_path" ]] || {
    usage
    exit 1
  }
  shift || true

  while (($#)); do
    case "$1" in
      --dry-run)
        dry_run=1
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

  [[ -f "$export_path" ]] || die "File not found: $export_path"
  command -v python3 >/dev/null 2>&1 || die "python3 is required."

  if jq empty "$export_path" >/dev/null 2>&1; then
    printf 'Already valid JSON: %s\n' "$export_path"
    exit 0
  fi

  python3 - "$export_path" "$dry_run" <<'PY'
import sys
from datetime import datetime, timezone
from pathlib import Path

path = Path(sys.argv[1])
dry_run = sys.argv[2] == "1"
data = path.read_bytes()
marker = b"},\n    {"
idx = data.rfind(marker)
if idx < 0:
    print("ERROR: could not find a safe message boundary to truncate", file=sys.stderr)
    sys.exit(1)

truncated = data[: idx + 1]
# idx+1 ends at closing brace of last complete message
suffix = b'\n  ],\n  "messageCount": 0\n}'
# preserve messageCount if we can count roughly - jq will fix on merge
out = truncated + suffix

if dry_run:
    print(f"Would salvage {path} ({len(data)} -> {len(out)} bytes)")
    sys.exit(0)

backup = path.with_suffix(path.suffix + f".bak.{datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')}")
backup.write_bytes(data)
path.write_bytes(out)
print(f"Backup: {backup}")
print(f"Salvaged: {path} ({len(data)} -> {len(out)} bytes)")
PY

  jq empty "$export_path" >/dev/null 2>&1 || die "Salvage did not produce valid JSON."

  local temp_file
  temp_file=$(mktemp "${TMPDIR:-/tmp}/dce-salvage.XXXXXX.json")
  jq '.messageCount = (.messages | length)' "$export_path" >"$temp_file"
  mv -f "$temp_file" "$export_path"

  local count
  count=$(jq -r '(.messages | length) // 0' "$export_path")
  printf 'Valid JSON with %s messages.\n' "$count"
}

main "$@"
