#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
HANDOFF="$REPO_ROOT/scripts/operator-handoff.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-handoff-smoke.XXXXXX")
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
handoff_output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    "$HANDOFF" --config "$CONFIG_PATH" --skip-df 2>&1
)
handoff_status=$?
set -e

if [[ "$handoff_status" -ne 0 ]] || ! grep -q 'Handoff complete' <<<"$handoff_output"; then
  printf 'operator-handoff failed (status=%s)\n' "$handoff_status" >&2
  printf '%s\n' "$handoff_output" >&2
  exit 1
fi

printf 'operator-handoff-smoke: ok\n'
