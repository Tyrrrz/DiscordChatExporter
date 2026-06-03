#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
STATUS="$REPO_ROOT/scripts/scrape-lock-status.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-lock-status-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
LOCK_FILE="$ARCHIVE_ROOT/.dce-scrape.lock"
HOLDER_PID=""

cleanup() {
  if [[ -n "$HOLDER_PID" ]] && kill -0 "$HOLDER_PID" 2>/dev/null; then
    kill "$HOLDER_PID" 2>/dev/null || true
    wait "$HOLDER_PID" 2>/dev/null || true
  fi
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

command -v flock >/dev/null 2>&1 || {
  echo "SKIP: flock not available"
  exit 0
}

mkdir -p "$ARCHIVE_ROOT"
cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": []
}
JSON

chmod +x "$STATUS"

set +e
free_output=$("$STATUS" --config "$CONFIG_PATH" 2>&1)
free_status=$?
set -e

if [[ "$free_status" -ne 0 ]] || ! grep -q 'state: free (no lock file)' <<<"$free_output"; then
  echo "expected free lock status" >&2
  printf '%s\n' "$free_output" >&2
  exit 1
fi
if ! grep -Fq "$LOCK_FILE" <<<"$free_output"; then
  echo "expected archive-root lock path in output" >&2
  exit 1
fi

(
  exec {lock_fd}>>"$LOCK_FILE"
  flock -n "$lock_fd" || exit 1
  printf 'pid=%s\nstarted=2020-01-01T00:00:00Z\ncmd=lock-status-smoke-holder\n' "$$" >"${LOCK_FILE}.meta"
  sleep 120
) &
HOLDER_PID=$!
sleep 0.2

set +e
held_output=$("$STATUS" --config "$CONFIG_PATH" 2>&1)
held_status=$?
set -e

if [[ "$held_status" -ne 1 ]] || ! grep -q 'state: held (active scrape)' <<<"$held_output"; then
  echo "expected held lock status exit 1" >&2
  printf '%s\n' "$held_output" >&2
  exit 1
fi
if ! grep -q 'lock-status-smoke-holder' <<<"$held_output"; then
  echo "expected holder cmd in status output" >&2
  exit 1
fi

kill "$HOLDER_PID" 2>/dev/null || true
wait "$HOLDER_PID" 2>/dev/null || true
HOLDER_PID=""

printf 'pid=99999999\nstarted=2020-01-01T00:00:00Z\ncmd=dead-smoke-holder\n' >"${LOCK_FILE}.meta"
touch "$LOCK_FILE"

set +e
stale_output=$("$STATUS" --config "$CONFIG_PATH" 2>&1)
stale_status=$?
set -e

if [[ "$stale_status" -ne 0 ]] || ! grep -q 'state: stale (reclaimable' <<<"$stale_output"; then
  echo "expected stale reclaimable status after holder exit" >&2
  printf '%s\n' "$stale_output" >&2
  exit 1
fi

set +e
reclaim_output=$("$STATUS" --config "$CONFIG_PATH" --reclaim-stale 2>&1)
reclaim_status=$?
set -e

if [[ "$reclaim_status" -ne 0 ]] || ! grep -q 'removed stale lock meta' <<<"$reclaim_output"; then
  echo "expected --reclaim-stale to remove stale meta" >&2
  printf '%s\n' "$reclaim_output" >&2
  exit 1
fi
[[ ! -f "${LOCK_FILE}.meta" ]] || {
  echo "expected stale meta removed after reclaim" >&2
  exit 1
}

printf 'scrape-lock-status-smoke: ok\n'
