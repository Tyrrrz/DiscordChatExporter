#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
VERIFY="$REPO_ROOT/scripts/verify-operator-ready.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-disk-smoke.XXXXXX")
CONFIG_PATH="$TMP_DIR/config.json"
ENV_PATH="$TMP_DIR/scrape.env"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$TMP_DIR/archive/demo"
printf '{"messages":[{"id":"1"}],"channel":{"id":"111111111111111111"}}\n' \
  >"$TMP_DIR/archive/demo/Guild - general [111111111111111111].json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$TMP_DIR/archive",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$TMP_DIR/archive/demo",
      "enabled": true
    }
  ]
}
JSON

printf 'DISCORD_TOKEN=dummy\n' >"$ENV_PATH"

set +e
output=$(
  DCE_MIN_FREE_MB=999999999 \
    DCE_REPO_ROOT="$REPO_ROOT" \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    "$VERIFY" --config "$CONFIG_PATH" 2>&1
)
verify_status=$?
set -e

if (( verify_status != 0 )) && printf '%s\n' "$output" | grep -qi 'Insufficient disk space'; then
  printf 'archive-disk-space-smoke: ok\n'
  exit 0
fi

printf 'ERROR: expected disk space check to fail with high DCE_MIN_FREE_MB (status=%s)\n' "$verify_status" >&2
printf '%s\n' "$output" >&2
exit 1
