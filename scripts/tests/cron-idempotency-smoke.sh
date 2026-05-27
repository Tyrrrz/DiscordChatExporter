#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
CRONTAB_DIR="$REPO_ROOT/scripts/tests/test-crontabs"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-cron-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
ENV_PATH="$TMP_DIR/scrape.env"
CRONTAB_FILE="$TMP_DIR/crontab.txt"
DOCKER_LOG="$TMP_DIR/docker.log"
FAKE_DOCKER="$TMP_DIR/docker"
FAKE_CRONTAB="$TMP_DIR/crontab"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE_ROOT"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "defaults": {
    "include_threads": "all",
    "include_voice_channels": false
  },
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    }
  ]
}
JSON

cat >"$ENV_PATH" <<EOF
DISCORD_TOKEN=dummy
DCE_UID=1000
DCE_GID=1000
TZ=UTC
EOF

cat >"$FAKE_DOCKER" <<EOF
#!/usr/bin/env bash
set -Eeuo pipefail
printf '%s\n' "\$*" >>"$DOCKER_LOG"
exit 0
EOF
chmod +x "$FAKE_DOCKER"

cat >"$FAKE_CRONTAB" <<EOF
#!/usr/bin/env bash
set -Eeuo pipefail
file="$CRONTAB_FILE"
if [[ "\${1:-}" == "-l" ]]; then
  [[ -f "\$file" ]] || exit 1
  cat "\$file"
elif [[ "\${1:-}" == "-" ]]; then
  cat >"\$file"
else
  echo "unexpected crontab args: \$*" >&2
  exit 1
fi
EOF
chmod +x "$FAKE_CRONTAB"

cp "$CRONTAB_DIR/fixture-with-unrelated-entries.txt" "$CRONTAB_FILE"

run_setup() {
  DCE_CONFIG_FILE="$CONFIG_PATH" \
  DCE_ENV_FILE="$ENV_PATH" \
  DCE_CRONTAB_BIN="$FAKE_CRONTAB" \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_JQ_BIN="$(command -v jq)" \
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_LOG_FILE="$TMP_DIR/logs/discord-scrape.log" \
  "$REPO_ROOT/scripts/setup-cron.sh" --target demo "$@"
}

echo "Test 1: Initial cron install preserves unrelated entries..."
run_setup
grep -q '^0 10 \* \* \* /usr/sbin/sendmail -q$' "$CRONTAB_FILE" || {
  echo "  FAIL: unrelated sendmail entry missing after install" >&2
  exit 1
}
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || {
  echo "  FAIL: expected exactly one managed cron block after install" >&2
  exit 1
}
echo "  PASS: install adds one managed block and keeps unrelated entries"

echo ""
echo "Test 2: Reinstall with same config is idempotent..."
run_setup
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || {
  echo "  FAIL: expected exactly one managed cron block after reinstall" >&2
  exit 1
}
echo "  PASS: reinstall leaves a single managed block"

echo ""
echo "Test 3: Schedule update replaces only the managed block..."
run_setup --interval weekly --at 03:15
grep -q '^0 10 \* \* \* /usr/sbin/sendmail -q$' "$CRONTAB_FILE" || {
  echo "  FAIL: unrelated sendmail entry missing after schedule update" >&2
  exit 1
}
grep -q '15 03 \* \* 0' "$CRONTAB_FILE" || {
  echo "  FAIL: expected weekly schedule in managed block" >&2
  exit 1
}
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || {
  echo "  FAIL: expected exactly one managed cron block after schedule update" >&2
  exit 1
}
echo "  PASS: schedule update changes managed block only"

echo ""
echo "Test 4: Dry-run previews managed block without mutating crontab..."
before_hash=$(sha256sum "$CRONTAB_FILE" | awk '{print $1}')
preview_output=$(run_setup --dry-run)
grep -q '^# BEGIN discord-scrape$' <<<"$preview_output" || {
  echo "  FAIL: dry-run preview missing managed block" >&2
  exit 1
}
after_hash=$(sha256sum "$CRONTAB_FILE" | awk '{print $1}')
[[ "$before_hash" == "$after_hash" ]] || {
  echo "  FAIL: dry-run altered crontab state" >&2
  exit 1
}
echo "  PASS: dry-run is read-only"

echo ""
echo "Test 5: Remove deletes managed block and keeps unrelated entries..."
run_setup --remove
grep -q '^0 10 \* \* \* /usr/sbin/sendmail -q$' "$CRONTAB_FILE" || {
  echo "  FAIL: unrelated sendmail entry missing after remove" >&2
  exit 1
}
! grep -q '^# BEGIN discord-scrape$' "$CRONTAB_FILE" || {
  echo "  FAIL: managed cron block still present after remove" >&2
  exit 1
}
echo "  PASS: remove clears managed block only"

echo ""
echo "U3: cron idempotency smoke test passed"
