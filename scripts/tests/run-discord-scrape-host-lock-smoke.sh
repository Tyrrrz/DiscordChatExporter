#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-host-lock-smoke.XXXXXX")
ENV_FILE="$TMP_DIR/scrape.env"
COMPOSE_FILE="$TMP_DIR/docker-compose.yml"
FAKE_DOCKER="$TMP_DIR/docker"
LOCK_FILE="$TMP_DIR/scrape.lock"
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

cat >"$ENV_FILE" <<EOF
DISCORD_TOKEN=dummy-token
EOF

(
  exec {lock_fd}>>"$LOCK_FILE"
  flock -n "$lock_fd" || exit 1
  printf 'pid=%s\nstarted=2020-01-01T00:00:00Z\ncmd=mock-lock-holder\n' "$$" >"${LOCK_FILE}.meta"
  sleep 120
) &
HOLDER_PID=$!
sleep 0.2

set +e
output=$(
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_SCRAPE_LOCK_FILE="$LOCK_FILE" \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$ENV_FILE" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo 2>&1
)
status=$?
set -e

if [[ "$status" -eq 0 ]]; then
  echo "expected scrape to fail while lock is held" >&2
  exit 1
fi
if ! grep -q 'Another scrape is already running' <<<"$output"; then
  echo "expected lock-held error message" >&2
  printf '%s\n' "$output" >&2
  exit 1
fi
if ! grep -q 'Holder pid' <<<"$output"; then
  echo "expected holder pid details in lock-held error" >&2
  printf '%s\n' "$output" >&2
  exit 1
fi

kill "$HOLDER_PID" 2>/dev/null || true
wait "$HOLDER_PID" 2>/dev/null || true
HOLDER_PID=""

if ! DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_SCRAPE_LOCK_FILE="$LOCK_FILE" \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$ENV_FILE" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo >/dev/null; then
  echo "expected scrape to succeed after lock released" >&2
  exit 1
fi

ARCHIVE_ROOT="$TMP_DIR/archive_shared"
mkdir -p "$ARCHIVE_ROOT/demo"
CONFIG_PATH="$TMP_DIR/archive-config.json"
ARCHIVE_LOCK="$ARCHIVE_ROOT/.dce-scrape.lock"
cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "enabled": true
    }
  ]
}
JSON

(
  exec {archive_lock_fd}>>"$ARCHIVE_LOCK"
  flock -n "$archive_lock_fd" || exit 1
  sleep 120
) &
HOLDER_PID=$!
sleep 0.2

set +e
archive_output=$(
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$ENV_FILE" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --config "$CONFIG_PATH" --target demo 2>&1
)
archive_status=$?
set -e

kill "$HOLDER_PID" 2>/dev/null || true
wait "$HOLDER_PID" 2>/dev/null || true
HOLDER_PID=""

if [[ "$archive_status" -eq 0 ]]; then
  echo "expected archive-root lock to block scrape" >&2
  exit 1
fi
if ! grep -Fq "$ARCHIVE_LOCK" <<<"$archive_output"; then
  echo "expected archive-root lock path in error message" >&2
  printf '%s\n' "$archive_output" >&2
  exit 1
fi

echo "run-discord-scrape-host-lock-smoke: OK"
