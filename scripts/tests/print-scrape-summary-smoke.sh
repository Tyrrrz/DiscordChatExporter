#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
PRINT="$REPO_ROOT/scripts/print-scrape-summary.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-print-summary-smoke.XXXXXX")
trap 'rm -rf "$TMP_DIR"' EXIT

FIXTURE="$TMP_DIR/fixture.summary.json"
cat >"$FIXTURE" <<'JSON'
{
  "version": 1,
  "finished_at": "2026-06-04T12:00:00Z",
  "totals": {
    "created": 0,
    "merged": 1,
    "unchanged": 1,
    "skipped": 1,
    "skipped_oom": 1,
    "messages_appended": 5
  },
  "channels": [
    {
      "target": "demo",
      "channel_id": "111111111111111111",
      "guild_label": "general",
      "file_path": "/tmp/archive/demo/general.json",
      "action": "MERGED",
      "before_count": 10,
      "fetched_count": 3,
      "after_count": 13,
      "delta": 3
    },
    {
      "target": "demo",
      "channel_id": "222222222222222222",
      "guild_label": "other",
      "file_path": "/tmp/archive/demo/other.json",
      "action": "UNCHANGED",
      "before_count": 5,
      "fetched_count": 0,
      "after_count": 5,
      "delta": 0
    },
    {
      "target": "demo",
      "channel_id": "333333333333333333",
      "guild_label": "big-channel",
      "file_path": "",
      "action": "SKIPPED_OOM",
      "before_count": 0,
      "fetched_count": 0,
      "after_count": 0,
      "delta": 0
    }
  ]
}
JSON

chmod +x "$PRINT"

output=$("$PRINT" "$FIXTURE")
grep -q 'Scrape summary (finished 2026-06-04T12:00:00Z)' <<<"$output" || {
  printf 'ERROR: missing finished_at header\n' >&2
  printf '%s\n' "$output" >&2
  exit 1
}
grep -q '+5 messages appended' <<<"$output" || {
  printf 'ERROR: missing totals line\n' >&2
  exit 1
}
grep -q 'MERGED' <<<"$output" || {
  printf 'ERROR: missing MERGED channel row\n' >&2
  exit 1
}
grep -q 'SKIPPED(OOM)' <<<"$output" || {
  printf 'ERROR: missing SKIPPED(OOM) channel row\n' >&2
  exit 1
}

json_out=$("$PRINT" --json "$FIXTURE")
diff -q "$FIXTURE" <(printf '%s\n' "$json_out") >/dev/null || {
  printf 'ERROR: --json output differs from source file\n' >&2
  exit 1
}

oom_out=$("$PRINT" --oom-only "$FIXTURE")
grep -q '333333333333333333' <<<"$oom_out" || {
  printf 'ERROR: --oom-only missing OOM channel\n' >&2
  exit 1
}
grep -q '111111111111111111' <<<"$oom_out" && {
  printf 'ERROR: --oom-only should exclude non-OOM channels\n' >&2
  exit 1
}

stdin_out=$(cat "$FIXTURE" | "$PRINT" -)
grep -q 'MERGED' <<<"$stdin_out" || {
  printf 'ERROR: stdin (-) mode failed\n' >&2
  exit 1
}

if "$PRINT" "$TMP_DIR/missing.json" >/dev/null 2>&1; then
  printf 'ERROR: expected non-zero exit for missing file\n' >&2
  exit 1
fi

printf '{"version":2}\n' >"$TMP_DIR/bad.summary.json"
if "$PRINT" "$TMP_DIR/bad.summary.json" >/dev/null 2>&1; then
  printf 'ERROR: expected non-zero exit for invalid schema\n' >&2
  exit 1
fi

printf 'print-scrape-summary-smoke: ok\n'
