#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
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

printf 'MAILTO=test@example.com\n' >"$CRONTAB_FILE"

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

run_setup
grep -q '^MAILTO=test@example.com$' "$CRONTAB_FILE" || { echo "expected unrelated crontab line to remain" >&2; exit 1; }
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || { echo "expected exactly one managed cron block after install" >&2; exit 1; }
grep -q 'compose --env-file' "$DOCKER_LOG" || { echo "expected docker preflight to run during install" >&2; exit 1; }
grep -q 'scripts/run-documents-scrape.sh' "$CRONTAB_FILE" || { echo "expected cron job to run documents scrape" >&2; exit 1; }
grep -q -- '--log-file' "$CRONTAB_FILE" || { echo "expected cron job to pass --log-file" >&2; exit 1; }
grep -q 'DCE_ENV_FILE=' "$CRONTAB_FILE" || { echo "expected cron job to set DCE_ENV_FILE" >&2; exit 1; }
grep -q 'DCE_COMPOSE_TTY=0' "$CRONTAB_FILE" || { echo "expected cron job to disable compose TTY" >&2; exit 1; }
grep -q '>>' "$CRONTAB_FILE" && { echo "expected cron job to rely on documents scrape tee, not shell redirect" >&2; exit 1; }

run_setup
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || { echo "expected exactly one managed cron block after reinstall" >&2; exit 1; }

preview_output=$(run_setup --dry-run)
grep -q '^# BEGIN discord-scrape$' <<<"$preview_output" || { echo "expected dry-run preview to contain managed block" >&2; exit 1; }
[[ "$(grep -c '^# BEGIN discord-scrape$' "$CRONTAB_FILE")" == "1" ]] || { echo "dry-run should not alter crontab state" >&2; exit 1; }

run_setup --remove
grep -q '^MAILTO=test@example.com$' "$CRONTAB_FILE" || { echo "expected unrelated crontab line to remain after remove" >&2; exit 1; }
! grep -q '^# BEGIN discord-scrape$' "$CRONTAB_FILE" || { echo "expected managed cron block to be removed" >&2; exit 1; }

printf 'MAILTO=test@example.com\n' >"$CRONTAB_FILE"
cp "$CRONTAB_FILE" "$TMP_DIR/crontab-before-preflight-fail.txt"
FAKE_FAIL_DOCKER="$TMP_DIR/docker-fail"
cat >"$FAKE_FAIL_DOCKER" <<EOF
#!/usr/bin/env bash
set -Eeuo pipefail
if [[ "\${1:-}" == "compose" ]]; then
  echo "simulated compose build failure" >&2
  exit 1
fi
exit 0
EOF
chmod +x "$FAKE_FAIL_DOCKER"

if DCE_CONFIG_FILE="$CONFIG_PATH" \
   DCE_ENV_FILE="$ENV_PATH" \
   DCE_CRONTAB_BIN="$FAKE_CRONTAB" \
   DCE_DOCKER_BIN="$FAKE_FAIL_DOCKER" \
   DCE_JQ_BIN="$(command -v jq)" \
   DCE_REPO_ROOT="$REPO_ROOT" \
   DCE_LOG_FILE="$TMP_DIR/logs/discord-scrape-fail.log" \
   "$REPO_ROOT/scripts/setup-cron.sh" --target demo 2>/dev/null; then
  echo "expected setup to fail when preflight docker build fails" >&2
  exit 1
fi

cmp -s "$CRONTAB_FILE" "$TMP_DIR/crontab-before-preflight-fail.txt" || {
  echo "expected crontab to remain unchanged when preflight fails" >&2
  exit 1
}

if run_setup --cron "0 2 * * *; touch /tmp/evil" --skip-preflight 2>/dev/null; then
  echo "expected invalid --cron expression to fail validation" >&2
  exit 1
fi

preview_custom_cron=$(run_setup --cron "15 03 * * 0" --skip-preflight --dry-run)
grep -q '^15 03 \* \* 0 ' <<<"$preview_custom_cron" || { echo "expected validated custom cron in dry-run output" >&2; exit 1; }

echo "setup-cron smoke test passed"
