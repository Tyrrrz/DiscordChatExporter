#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-audit-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
AUDIT="$REPO_ROOT/scripts/audit-archive-json.sh"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE_ROOT/good" "$ARCHIVE_ROOT/bad"

cat >"$ARCHIVE_ROOT/good/valid [111].json" <<'JSON'
{"guild":{"id":"1","name":"g"},"channel":{"id":"111","name":"c"},"messages":[],"messageCount":0}
JSON

printf '{"messages":[\n' >"$ARCHIVE_ROOT/bad/truncated [222].json"

mkdir -p "$ARCHIVE_ROOT/good/.dce-temp/export.111.PARTIAL"
printf '{"messages":[\n' >"$ARCHIVE_ROOT/good/.dce-temp/export.111.PARTIAL/export.json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/good",
      "enabled": true
    },
    {
      "name": "broken",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/bad",
      "enabled": true
    }
  ]
}
JSON

DCE_CONFIG_FILE="$CONFIG_PATH" "$AUDIT" --target demo

set +e
broken_output=$(DCE_CONFIG_FILE="$CONFIG_PATH" "$AUDIT" --target broken 2>&1)
broken_status=$?
set -e
if [[ "$broken_status" -eq 0 ]]; then
  printf 'ERROR: audit should fail for target with invalid JSON\n' >&2
  exit 1
fi
if ! grep -q 'INVALID' <<<"$broken_output"; then
  printf 'ERROR: audit output missing INVALID marker\n' >&2
  exit 1
fi

printf 'audit-archive-json-smoke: OK\n'
