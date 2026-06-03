#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
PROOF="$REPO_ROOT/scripts/run-operator-proof.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-operator-proof-smoke.XXXXXX")
CONFIG_PATH="$TMP_DIR/config.json"
ENV_PATH="$TMP_DIR/scrape.env"
mkdir -p "$TMP_DIR/logs"

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

set +e
salvage_output=$(
  DCE_MIN_FREE_MB=0 \
    DCE_CONFIG_FILE="$CONFIG_PATH" \
    DCE_ENV_FILE="$ENV_PATH" \
    DCE_SKIP_SCRAPE_LOCK=1 \
    "$PROOF" --config "$CONFIG_PATH" --target demo --salvage-only 2>&1
)
salvage_status=$?
set -e

if [[ "$salvage_status" -ne 0 ]] || ! grep -q 'Salvage-only proof complete' <<<"$salvage_output"; then
  printf 'run-operator-proof --salvage-only failed (status=%s)\n' "$salvage_status" >&2
  printf '%s\n' "$salvage_output" >&2
  exit 1
fi

command -v flock >/dev/null 2>&1 && {
  LOCK_FILE="$TMP_DIR/archive/.dce-scrape.lock"
  HOLDER_PID=""
  (
    exec {lock_fd}>>"$LOCK_FILE"
    flock -n "$lock_fd" || exit 1
    sleep 120
  ) &
  HOLDER_PID=$!
  sleep 0.2

  set +e
  blocked_output=$(
    DCE_MIN_FREE_MB=0 \
      DCE_CONFIG_FILE="$CONFIG_PATH" \
      DCE_ENV_FILE="$ENV_PATH" \
      DCE_LOG_DIR="$TMP_DIR/logs" \
      "$REPO_ROOT/scripts/run-operator-validation.sh" \
      --salvage-only --target demo --config "$CONFIG_PATH" \
      --log-file "$TMP_DIR/logs/lock-blocked.log" 2>&1
  )
  blocked_status=$?
  set -e

  kill "$HOLDER_PID" 2>/dev/null || true
  wait "$HOLDER_PID" 2>/dev/null || true

  if [[ "$blocked_status" -eq 0 ]] || ! grep -q 'Scrape lock is held' <<<"$blocked_output"; then
    printf 'expected validation to fail when scrape lock held (status=%s)\n' "$blocked_status" >&2
    printf '%s\n' "$blocked_output" >&2
    exit 1
  fi
}

printf 'run-operator-proof-smoke: ok\n'
