#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
PROVE="$REPO_ROOT/scripts/prove-incremental-append.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-prove-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
BEFORE="$TMP_DIR/before.tsv"
AFTER="$TMP_DIR/after.tsv"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE_ROOT/demo"

cat >"$ARCHIVE_ROOT/demo/Guild - general [111111111111111111].json" <<'JSON'
{
  "guild": {"id": "1", "name": "Guild"},
  "channel": {"id": "111111111111111111", "name": "general"},
  "messages": [
    {"id": "1", "timestamp": "2020-01-01T00:00:00+00:00", "type": "Default", "content": "one"}
  ],
  "messageCount": 1
}
JSON

printf '{"messages":[\n' >"$ARCHIVE_ROOT/demo/truncated [222222222222222222].json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "enabled": true
    }
  ]
}
JSON

DCE_PRIMARY_CONFIG="$CONFIG_PATH" "$PROVE" --target demo --snapshot-only --snapshot-file "$BEFORE"

if ! grep -q '111111111111111111' "$BEFORE"; then
  printf 'ERROR: snapshot missing valid channel archive\n' >&2
  exit 1
fi
if grep -q '222222222222222222' "$BEFORE"; then
  printf 'ERROR: invalid JSON file should be skipped in snapshot\n' >&2
  exit 1
fi

cat >"$ARCHIVE_ROOT/demo/Guild - general [111111111111111111].json" <<'JSON'
{
  "guild": {"id": "1", "name": "Guild"},
  "channel": {"id": "111111111111111111", "name": "general"},
  "messages": [
    {"id": "1", "timestamp": "2020-01-01T00:00:00+00:00", "type": "Default", "content": "one"},
    {"id": "2", "timestamp": "2020-01-02T00:00:00+00:00", "type": "Default", "content": "two"}
  ],
  "messageCount": 2
}
JSON

DCE_PRIMARY_CONFIG="$CONFIG_PATH" "$PROVE" --target demo --snapshot-only --snapshot-file "$AFTER"
"$PROVE" --compare-snapshots "$BEFORE" "$AFTER"

if "$PROVE" --compare-snapshots "$AFTER" "$BEFORE" 2>/dev/null; then
  printf 'ERROR: shrink comparison should have failed\n' >&2
  exit 1
fi

printf 'prove-incremental-append-smoke: ok\n'
