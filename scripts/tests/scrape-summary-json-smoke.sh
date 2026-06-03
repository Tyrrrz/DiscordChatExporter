#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
# shellcheck source=../lib/scrape-summary-json.sh
source "$REPO_ROOT/scripts/lib/scrape-summary-json.sh"

TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-summary-json-smoke.XXXXXX")
trap 'rm -rf "$TMP_DIR"' EXIT

LOG_FILE="$TMP_DIR/scrape.log"
OUT_FILE="$TMP_DIR/recovered.summary.json"

cat >"$LOG_FILE" <<'LOG'
[2026-06-04T12:00:00Z] scrape started
[2026-06-04T12:01:00Z] DCE_JSON_SUMMARY: {"version":1,"totals":{"created":0,"merged":1,"unchanged":2,"skipped":0,"skipped_oom":0,"messages_appended":3}}
LOG

extract_json_summary_from_log "$LOG_FILE" "$OUT_FILE" || {
  printf 'ERROR: expected extract to succeed on valid marker line\n' >&2
  exit 1
}

[[ -s "$OUT_FILE" ]] || {
  printf 'ERROR: recovered summary file missing\n' >&2
  exit 1
}

jq -e '.totals.merged == 1 and .totals.messages_appended == 3' "$OUT_FILE" >/dev/null || {
  printf 'ERROR: recovered JSON content mismatch\n' >&2
  exit 1
}

printf '[2026-06-04T12:02:00Z] DCE_JSON_SUMMARY: {"version":1,"totals":{"merged":9}}\n' >>"$LOG_FILE"
extract_json_summary_from_log "$LOG_FILE" "$OUT_FILE" || {
  printf 'ERROR: expected second extract to succeed\n' >&2
  exit 1
}

jq -e '.totals.merged == 9' "$OUT_FILE" >/dev/null || {
  printf 'ERROR: expected last DCE_JSON_SUMMARY line to win\n' >&2
  exit 1
}

EXISTING="$TMP_DIR/existing.summary.json"
printf '{"version":1,"totals":{"merged":1}}\n' >"$EXISTING"
if recover_json_summary_if_missing "$LOG_FILE" "$EXISTING" 2>/dev/null; then
  printf 'ERROR: recover should skip when dest already non-empty\n' >&2
  exit 1
fi

RECOVER_OUT="$TMP_DIR/recover-via-helper.summary.json"
recover_json_summary_if_missing "$LOG_FILE" "$RECOVER_OUT" || {
  printf 'ERROR: recover_json_summary_if_missing failed\n' >&2
  exit 1
}
jq -e '.totals.merged == 9' "$RECOVER_OUT" >/dev/null || {
  printf 'ERROR: recover helper wrote wrong content\n' >&2
  exit 1
}

if extract_json_summary_from_log "$TMP_DIR/missing.log" "$OUT_FILE" 2>/dev/null; then
  printf 'ERROR: extract should fail on missing log\n' >&2
  exit 1
fi

printf '[2026-06-04T12:03:00Z] no summary here\n' >"$TMP_DIR/empty.log"
if extract_json_summary_from_log "$TMP_DIR/empty.log" "$OUT_FILE" 2>/dev/null; then
  printf 'ERROR: extract should fail when marker absent\n' >&2
  exit 1
fi

printf '[2026-06-04T12:04:00Z] DCE_JSON_SUMMARY: not-json\n' >"$TMP_DIR/bad.log"
if extract_json_summary_from_log "$TMP_DIR/bad.log" "$OUT_FILE" 2>/dev/null; then
  printf 'ERROR: extract should fail on invalid JSON\n' >&2
  exit 1
fi

path=$(per_target_summary_file "$TMP_DIR" operator-validation 'KotOR_discord_msgs')
[[ "$path" == "$TMP_DIR/operator-validation-KotOR_discord_msgs-"*.summary.json ]] || {
  printf 'ERROR: unexpected per_target_summary_file path: %s\n' "$path" >&2
  exit 1
}

slug_path=$(per_target_summary_file "$TMP_DIR" operator-proof 'weird name!')
[[ "$slug_path" == "$TMP_DIR/operator-proof-weird_name_-"*.summary.json ]] || {
  printf 'ERROR: expected sanitized slug in path: %s\n' "$slug_path" >&2
  exit 1
}

printf 'scrape-summary-json-smoke: ok\n'
