#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
PROOF="$REPO_ROOT/scripts/run-operator-proof.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-operator-proof-smoke.XXXXXX")
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
      "channel_ids": ["111111111111111111"],
      "enabled": true
    }
  ]
}
JSON

printf 'DISCORD_TOKEN=dummy\n' >"$ENV_PATH"

set +e
output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    "$PROOF" --config "$CONFIG_PATH" --target demo --dry-run 2>&1
)
status=$?
set -e

if [[ "$status" -ne 0 ]] || ! grep -q 'Dry run complete' <<<"$output"; then
  printf 'run-operator-proof dry-run failed (status=%s)\n' "$status" >&2
  printf '%s\n' "$output" >&2
  exit 1
fi
grep -q 'Operator proof run plan' <<<"$output" || {
  echo "expected Operator proof run plan in dry-run output" >&2
  exit 1
}

printf 'run-operator-proof-smoke: ok\n'
