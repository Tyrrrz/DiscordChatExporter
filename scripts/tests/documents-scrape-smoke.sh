#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-documents-scrape-smoke.XXXXXX")

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

FAKE_REPO="$TMP_DIR/fake-repo"
mkdir -p "$FAKE_REPO/scripts/lib"
cp "$REPO_ROOT/scripts/run-discord-scrape-host.sh" "$FAKE_REPO/scripts/"
cp "$REPO_ROOT/scripts/lib/scrape-run-plan.sh" "$FAKE_REPO/scripts/lib/"
cp "$REPO_ROOT/scripts/lib/scrape-lock.sh" "$FAKE_REPO/scripts/lib/"
chmod +x "$FAKE_REPO/scripts/run-discord-scrape-host.sh"

COMPOSE_FILE="$TMP_DIR/docker-compose.yml"
FAKE_DOCKER="$TMP_DIR/docker"
CALL_COUNT="$TMP_DIR/call-count"

cat >"$COMPOSE_FILE" <<'EOF'
services:
  discord-scraper:
    image: fake
EOF

cat >"$FAKE_DOCKER" <<'EOF'
#!/usr/bin/env bash
printf 'run succeeded\n'
EOF
chmod +x "$FAKE_DOCKER"

printf 'discovered-token\n' >"$FAKE_REPO/.discord-token"
MISSING_ENV="$TMP_DIR/missing-scrape.env"
[[ ! -e "$MISSING_ENV" ]]

DCE_REPO_ROOT="$FAKE_REPO" \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$MISSING_ENV" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  FAKE_DOCKER_CALL_COUNT="$CALL_COUNT" \
  "$FAKE_REPO/scripts/run-discord-scrape-host.sh" scrape --target demo >/dev/null

ARCHIVE="$TMP_DIR/server"
mkdir -p "$ARCHIVE"
printf '{"guild":{"id":"1","name":"Guild"},"channel":{"id":"111111111111111111","name":"general"},"messages":[{"id":"1","timestamp":"2020-01-01T00:00:00"}]}\n' >"$ARCHIVE/Guild - general [111111111111111111].json"

cat >"$TMP_DIR/config.json" <<JSON
{
  "archive_root": "$TMP_DIR",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE",
      "channel_ids": ["111111111111111111"],
      "guild_ids": [],
      "guild_name_patterns": []
    }
  ]
}
JSON

PROVE="$REPO_ROOT/scripts/prove-incremental-append.sh"
HOST="$REPO_ROOT/scripts/run-discord-scrape-host.sh"

# Prove script should fail when host would shrink archives (simulate by patching fake docker to no-op)
DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$MISSING_ENV" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  DISCORD_TOKEN=dummy \
  "$PROVE" --config "$TMP_DIR/config.json" --target demo >/dev/null

DOC_OUT="$TMP_DIR/documents-dry-run.log"
"$REPO_ROOT/scripts/run-documents-scrape.sh" --dry-run --config "$TMP_DIR/config.json" >"$DOC_OUT" 2>&1
grep -q 'Documents scrape run plan' "$DOC_OUT" || {
  echo "expected Documents scrape run plan in dry-run output" >&2
  exit 1
}
grep -q 'JSON summary file:' "$DOC_OUT" && {
  echo "dry-run should not enable JSON summary export" >&2
  exit 1
}

CHANNEL_DRY="$TMP_DIR/channel-dry-run.log"
"$REPO_ROOT/scripts/run-documents-scrape.sh" --dry-run --config "$TMP_DIR/config.json" --target demo --channel 111111111111111111 >"$CHANNEL_DRY" 2>&1
grep -q 'Documents scrape run plan' "$CHANNEL_DRY" || {
  echo "expected dry-run to accept --channel passthrough" >&2
  exit 1
}
grep -q 'JSON summary file:' "$CHANNEL_DRY" && {
  echo "dry-run with --channel should not enable JSON summary export" >&2
  exit 1
}

ARGS_LOG="$TMP_DIR/compose-args.log"
cat >"$FAKE_DOCKER" <<'EOF'
#!/usr/bin/env bash
printf '%s\n' "$*" >>"${FAKE_DOCKER_ARGS_LOG:?}"
printf 'run succeeded\n'
EOF
chmod +x "$FAKE_DOCKER"
printf 'DISCORD_TOKEN=dummy-token\n' >"$TMP_DIR/scrape.env"

LIVE_DOC_OUT="$TMP_DIR/documents-live.log"
DCE_MIN_FREE_MB=0 \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  FAKE_DOCKER_ARGS_LOG="$ARGS_LOG" \
  DCE_ENV_FILE="$TMP_DIR/scrape.env" \
  DCE_LOG_DIR="$TMP_DIR/logs" \
  "$REPO_ROOT/scripts/run-documents-scrape.sh" --config "$TMP_DIR/config.json" --target demo --channel 111111111111111111 >"$LIVE_DOC_OUT" 2>&1

grep -q 'JSON summary file:' "$LIVE_DOC_OUT" || {
  echo "expected live documents scrape to enable JSON summary export" >&2
  cat "$LIVE_DOC_OUT" >&2
  exit 1
}
grep -q '111111111111111111' "$ARGS_LOG" || {
  echo "expected --channel to reach container compose invocation" >&2
  cat "$ARGS_LOG" >&2
  exit 1
}

cp "$REPO_ROOT/scripts/run-discord-scrape.sh" "$FAKE_REPO/scripts/"
chmod +x "$FAKE_REPO/scripts/run-discord-scrape.sh"
SALVAGE_DOC_LOG="$TMP_DIR/salvage-documents.log"
DCE_MIN_FREE_MB=0 \
  DCE_SKIP_SCRAPE_LOCK=1 \
  "$REPO_ROOT/scripts/run-documents-scrape.sh" --salvage-only --config "$TMP_DIR/config.json" --target demo >"$SALVAGE_DOC_LOG" 2>&1 || {
  echo "salvage-only documents scrape failed" >&2
  cat "$SALVAGE_DOC_LOG" >&2
  exit 1
}
grep -q 'salvage completed' "$SALVAGE_DOC_LOG" || {
  echo "expected --salvage-only to run local salvage" >&2
  cat "$SALVAGE_DOC_LOG" >&2
  exit 1
}
grep -q 'JSON summary file:' "$SALVAGE_DOC_LOG" && {
  echo "salvage-only should not enable JSON summary export" >&2
  exit 1
}

SALVAGE_BEFORE_LOG="$TMP_DIR/salvage-before.log"
: >"$ARGS_LOG"
DCE_MIN_FREE_MB=0 \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  FAKE_DOCKER_ARGS_LOG="$ARGS_LOG" \
  DCE_ENV_FILE="$TMP_DIR/scrape.env" \
  "$REPO_ROOT/scripts/run-documents-scrape.sh" \
  --salvage-before-scrape --config "$TMP_DIR/config.json" --target demo >"$SALVAGE_BEFORE_LOG" 2>&1 || {
  echo "salvage-before-scrape documents scrape failed" >&2
  cat "$SALVAGE_BEFORE_LOG" >&2
  exit 1
}
grep -q 'salvage completed' "$SALVAGE_BEFORE_LOG" || {
  echo "expected --salvage-before-scrape to run local salvage first" >&2
  cat "$SALVAGE_BEFORE_LOG" >&2
  exit 1
}
grep -q 'compose' "$ARGS_LOG" || {
  echo "expected --salvage-before-scrape to continue into container scrape" >&2
  cat "$ARGS_LOG" >&2
  exit 1
}
grep -q 'JSON summary file:' "$SALVAGE_BEFORE_LOG" || {
  echo "expected --salvage-before-scrape live path to enable JSON summary export" >&2
  cat "$SALVAGE_BEFORE_LOG" >&2
  exit 1
}

command -v flock >/dev/null 2>&1 && {
  LOCK_FILE="$TMP_DIR/.dce-scrape.lock"
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
      "$REPO_ROOT/scripts/run-documents-scrape.sh" \
      --salvage-only --config "$TMP_DIR/config.json" --target demo 2>&1
  )
  blocked_status=$?
  set -e

  kill "$HOLDER_PID" 2>/dev/null || true
  wait "$HOLDER_PID" 2>/dev/null || true

  if [[ "$blocked_status" -eq 0 ]] || ! grep -q 'Scrape lock is held' <<<"$blocked_output"; then
    echo "expected documents scrape to fail when archive lock held" >&2
    printf '%s\n' "$blocked_output" >&2
    exit 1
  fi
}

DCE_MIN_FREE_MB=0 DCE_CONFIG_FILE="$TMP_DIR/config.json" \
  "$REPO_ROOT/scripts/verify-operator-ready.sh" --disk-only --config "$TMP_DIR/config.json" \
  | grep -q 'disk-only: ok'

echo "documents-scrape-smoke: ok"
