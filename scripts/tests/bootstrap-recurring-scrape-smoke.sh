#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
BOOTSTRAP="$REPO_ROOT/scripts/bootstrap-recurring-scrape.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-bootstrap-smoke.XXXXXX")

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

[[ -x "$BOOTSTRAP" ]] || {
  printf 'bootstrap-recurring-scrape.sh is not executable\n' >&2
  exit 1
}

"$BOOTSTRAP" --help | grep -q 'bootstrap-recurring-scrape' || {
  printf 'bootstrap --help missing expected text\n' >&2
  exit 1
}

mkdir -p "$TMP_DIR/archive/demo"
printf '{"messages":[{"id":"1","timestamp":"2020-01-01T00:00:00+00:00"}],"channel":{"id":"111111111111111111"}}\n' \
  >"$TMP_DIR/archive/demo/Guild - general [111111111111111111].json"

cat >"$TMP_DIR/config.json" <<JSON
{
  "archive_root": "$TMP_DIR",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$TMP_DIR/archive/demo",
      "channel_ids": ["111111111111111111"],
      "guild_ids": [],
      "guild_name_patterns": []
    }
  ]
}
JSON

set +e
bootstrap_output=$("$BOOTSTRAP" --dry-run --config "$TMP_DIR/config.json" 2>&1)
bootstrap_status=$?
set -e

if [[ "$bootstrap_status" -ne 0 ]] || ! grep -q 'Dry run complete' <<<"$bootstrap_output"; then
  printf 'bootstrap --dry-run failed (status=%s)\n' "$bootstrap_status" >&2
  printf '%s\n' "$bootstrap_output" >&2
  exit 1
fi

KOTOR_ARCHIVE="$TMP_DIR/archive/kotor"
mkdir -p "$KOTOR_ARCHIVE"
printf '{"messages":[{"id":"1","timestamp":"2020-01-01T00:00:00+00:00"}],"channel":{"id":"221726893064454144"}}\n' \
  >"$KOTOR_ARCHIVE/Guild - yes_general [221726893064454144].json"
KOTOR_CONFIG="$TMP_DIR/kotor-config.json"
cat >"$KOTOR_CONFIG" <<JSON
{
  "archive_root": "$TMP_DIR",
  "targets": [
    {
      "name": "KotOR_discord_msgs",
      "kind": "guild",
      "output_dir": "$KOTOR_ARCHIVE",
      "enabled": true
    }
  ]
}
JSON

set +e
kotor_output=$("$BOOTSTRAP" --dry-run --config "$KOTOR_CONFIG" 2>&1)
kotor_status=$?
set -e

if [[ "$kotor_status" -ne 0 ]] || ! grep -q 'Dry run complete' <<<"$kotor_output"; then
  printf 'bootstrap KotOR dry-run failed (status=%s)\n' "$kotor_status" >&2
  printf '%s\n' "$kotor_output" >&2
  exit 1
fi
grep -q 'run-kotor-yes-general-catchup.sh' <<<"$kotor_output" || {
  printf 'bootstrap dry-run missing KotOR catch-up hint\n' >&2
  exit 1
}

printf 'bootstrap-recurring-scrape-smoke: ok\n'
