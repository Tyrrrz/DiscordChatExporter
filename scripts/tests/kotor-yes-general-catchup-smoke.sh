#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
RUNNER="$REPO_ROOT/scripts/run-kotor-yes-general-catchup.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-kotor-catchup-smoke.XXXXXX")
ARCHIVE="$TMP_DIR/archive/kotor"
CONFIG_PATH="$TMP_DIR/config.json"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE"
printf '{"messages":[{"id":"1"}],"channel":{"id":"221726893064454144"}}\n' \
  >"$ARCHIVE/Guild - yes_general [221726893064454144].json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$TMP_DIR/archive",
  "targets": [
    {
      "name": "KotOR_discord_msgs",
      "kind": "guild",
      "output_dir": "$ARCHIVE",
      "enabled": true
    }
  ]
}
JSON

OUT="$TMP_DIR/dry-run.log"
"$RUNNER" --dry-run --config "$CONFIG_PATH" >"$OUT" 2>&1

grep -q 'KotOR_discord_msgs' "$OUT" || {
  printf 'ERROR: dry-run missing target in plan output\n' >&2
  cat "$OUT" >&2
  exit 1
}
grep -q '221726893064454144' "$RUNNER" || {
  printf 'ERROR: wrapper missing yes_general channel id\n' >&2
  exit 1
}
grep -q 'Documents scrape run plan' "$OUT" || {
  printf 'ERROR: dry-run missing documents scrape plan\n' >&2
  exit 1
}
grep -q 'JSON summary file:' "$OUT" && {
  printf 'ERROR: dry-run should not enable JSON summary\n' >&2
  exit 1
}

HELP=$("$RUNNER" --help 2>&1)
grep -q 'yes_general' <<<"$HELP" || {
  printf 'ERROR: help missing yes_general reference\n' >&2
  exit 1
}

printf 'kotor-yes-general-catchup-smoke: ok\n'
