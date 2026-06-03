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

set +e
channel_output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    "$HANDOFF" --config "$CONFIG_PATH" --skip-df --target demo --channel 111111111111111111 2>&1
)
channel_status=$?
set -e

if [[ "$channel_status" -ne 0 ]] || ! grep -q 'Handoff complete' <<<"$channel_output"; then
  printf 'operator-handoff --channel failed (status=%s)\n' "$channel_status" >&2
  printf '%s\n' "$channel_output" >&2
  exit 1
fi
if ! grep -q 'Scrape lock status' <<<"$handoff_output"; then
  printf 'operator-handoff missing scrape lock status section\n' >&2
  exit 1
fi

set +e
salvage_output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    DCE_SKIP_SCRAPE_LOCK=1 \
    "$HANDOFF" --config "$CONFIG_PATH" --skip-df --salvage-only --target demo 2>&1
)
salvage_status=$?
set -e

if [[ "$salvage_status" -ne 0 ]] || ! grep -q 'Handoff complete (salvage-only)' <<<"$salvage_output"; then
  printf 'operator-handoff --salvage-only failed (status=%s)\n' "$salvage_status" >&2
  printf '%s\n' "$salvage_output" >&2
  exit 1
fi
grep -q 'salvage completed' <<<"$salvage_output" || {
  printf 'operator-handoff --salvage-only missing salvage completed marker\n' >&2
  exit 1
}

KOTOR_CONFIG="$TMP_DIR/kotor-config.json"
KOTOR_ARCHIVE="$TMP_DIR/archive/kotor"
mkdir -p "$KOTOR_ARCHIVE"
printf '{"messages":[{"id":"1"}],"channel":{"id":"221726893064454144"}}\n' \
  >"$KOTOR_ARCHIVE/Guild - yes_general [221726893064454144].json"
cat >"$KOTOR_CONFIG" <<JSON
{
  "archive_root": "$TMP_DIR/archive",
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
kotor_output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$KOTOR_CONFIG" \
    DCE_ENV_FILE="$ENV_PATH" \
    "$HANDOFF" --config "$KOTOR_CONFIG" --skip-df 2>&1
)
kotor_status=$?
set -e

if [[ "$kotor_status" -ne 0 ]] || ! grep -q 'Handoff complete' <<<"$kotor_output"; then
  printf 'operator-handoff KotOR hint failed (status=%s)\n' "$kotor_status" >&2
  printf '%s\n' "$kotor_output" >&2
  exit 1
fi
grep -q 'run-kotor-yes-general-catchup.sh' <<<"$kotor_output" || {
  printf 'operator-handoff missing KotOR catch-up hint\n' >&2
  exit 1
}

printf 'operator-handoff-smoke: ok\n'
