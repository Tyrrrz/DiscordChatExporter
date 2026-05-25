#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-host-smoke.XXXXXX")
ENV_FILE="$TMP_DIR/scrape.env"
COMPOSE_FILE="$TMP_DIR/docker-compose.yml"
FAKE_DOCKER="$TMP_DIR/docker"
CALL_COUNT="$TMP_DIR/call-count"
TOKEN_FILE="$TMP_DIR/token.txt"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

cat >"$COMPOSE_FILE" <<'EOF'
services:
  discord-scraper:
    image: fake
EOF

cat >"$FAKE_DOCKER" <<'EOF'
#!/usr/bin/env bash
set -Eeuo pipefail

count_file=${FAKE_DOCKER_CALL_COUNT:?}
token_file=${FAKE_DOCKER_TOKEN_FILE:?}
mode=${FAKE_DOCKER_MODE:?}
count=0
if [[ -f "$count_file" ]]; then
  count=$(cat "$count_file")
fi
count=$((count + 1))
printf '%s' "$count" >"$count_file"

if [[ "$mode" == "auth-refresh" ]]; then
  if [[ "${DISCORD_TOKEN:-}" == "stale-token" ]]; then
    printf 'Authentication token is invalid.\n' >&2
    printf 'fresh-token\n' >"$token_file"
    exit 1
  fi
  printf 'run succeeded after refresh\n'
  exit 0
fi

if [[ "$mode" == "auth-persistent-fail" ]]; then
  printf "Request to 'channels/111' failed: forbidden.\n" >&2
  exit 1
fi

printf 'run succeeded\n'
EOF
chmod +x "$FAKE_DOCKER"

run_host() {
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$ENV_FILE" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  FAKE_DOCKER_CALL_COUNT="$CALL_COUNT" \
  FAKE_DOCKER_TOKEN_FILE="$TOKEN_FILE" \
  FAKE_DOCKER_MODE="$1" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo
}

printf 'stale-token\n' >"$TOKEN_FILE"
cat >"$ENV_FILE" <<EOF
DISCORD_TOKEN_FILE=$TOKEN_FILE
EOF

run_host auth-refresh >/dev/null
[[ "$(cat "$CALL_COUNT")" == "2" ]] || { echo "expected one retry after auth failure" >&2; exit 1; }

printf 'stale-token\n' >"$TOKEN_FILE"
printf '0' >"$CALL_COUNT"
if run_host auth-persistent-fail >/dev/null; then
  echo "expected persistent auth failure to exit non-zero" >&2
  exit 1
fi
[[ "$(cat "$CALL_COUNT")" == "2" ]] || { echo "expected exactly one retry before final failure" >&2; exit 1; }

echo "run-discord-scrape-host smoke test passed"
