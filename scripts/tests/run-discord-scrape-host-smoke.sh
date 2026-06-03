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

while (($#)); do
  case "$1" in
    --env-file)
      if [[ $# -ge 2 && -f "$2" ]]; then
        local_env=$2
        while IFS='=' read -r env_key env_value || [[ -n "$env_key" ]]; do
          [[ -z "$env_key" || "$env_key" =~ ^# ]] && continue
          env_key=${env_key#export }
          env_key=${env_key%%[[:space:]]*}
          printf -v "$env_key" '%s' "$env_value"
          export "$env_key"
        done <"$local_env"
      fi
      shift 2
      ;;
    *)
      shift
      ;;
  esac
done

token="${DISCORD_TOKEN:-}"
if [[ -z "$token" && -n "${DISCORD_TOKEN_FILE:-}" && -f "$DISCORD_TOKEN_FILE" ]]; then
  token=$(head -n 1 "$DISCORD_TOKEN_FILE" | tr -d '\r')
fi

if [[ "$mode" == "auth-refresh" ]]; then
  if [[ "$token" == "stale-token" ]]; then
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

if [[ "$mode" == "streaming" ]]; then
  printf 'streaming-line1\n'
  sleep 0.3
  printf 'streaming-line2\n'
  exit 0
fi

printf 'run succeeded\n'
EOF
chmod +x "$FAKE_DOCKER"

run_host() {
  local mode=$1
  local env_path=${2:-$ENV_FILE}

  env -u DISCORD_TOKEN \
    DCE_SKIP_SCRAPE_LOCK=1 \
    DCE_REPO_ROOT="$REPO_ROOT" \
    DCE_DOCKER_BIN="$FAKE_DOCKER" \
    DCE_ENV_FILE="$env_path" \
    DCE_COMPOSE_FILE="$COMPOSE_FILE" \
    FAKE_DOCKER_CALL_COUNT="$CALL_COUNT" \
    FAKE_DOCKER_TOKEN_FILE="$TOKEN_FILE" \
    FAKE_DOCKER_MODE="$mode" \
    "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo
}

run_host_compose_capture() {
  local env_path=${1:-$ENV_FILE}
  local compose_bin=$2
  local args_log=$3
  shift 3
  local -a extra_env=( "$@" )

  env -u DISCORD_TOKEN \
    DCE_SKIP_SCRAPE_LOCK=1 \
    DCE_COMPOSE_BIN="$compose_bin" \
    DCE_REPO_ROOT="$REPO_ROOT" \
    DCE_ENV_FILE="$env_path" \
    DCE_COMPOSE_FILE="$COMPOSE_FILE" \
    FAKE_COMPOSE_ARGS_LOG="$args_log" \
    "${extra_env[@]}" \
    "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo
}

run_host_with_shell_token() {
  local mode=$1
  local missing_env_path=$2

  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$missing_env_path" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  DISCORD_TOKEN=dummy-token \
  FAKE_DOCKER_CALL_COUNT="$CALL_COUNT" \
  FAKE_DOCKER_TOKEN_FILE="$TOKEN_FILE" \
  FAKE_DOCKER_MODE="$mode" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape --target demo
}

MALICIOUS_ENV="$TMP_DIR/malicious.env"
MARKER_FILE="$TMP_DIR/marker"
cat >"$MALICIOUS_ENV" <<EOF
DISCORD_TOKEN=dummy
MALICIOUS=\$(touch "$MARKER_FILE")
EOF
run_host success "$MALICIOUS_ENV" >/dev/null
[[ ! -e "$MARKER_FILE" ]] || { echo "env parsing executed shell payload unexpectedly" >&2; exit 1; }

printf 'stale-token\n' >"$TOKEN_FILE"
printf '0' >"$CALL_COUNT"
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

MISSING_ENV="$TMP_DIR/missing-scrape.env"
[[ ! -e "$MISSING_ENV" ]]
printf '0' >"$CALL_COUNT"
run_host_with_shell_token success "$MISSING_ENV" >/dev/null
[[ "$(cat "$CALL_COUNT")" == "1" ]] || { echo "expected host wrapper to run with exported DISCORD_TOKEN when scrape.env is missing" >&2; exit 1; }

STREAM_OUTPUT="$TMP_DIR/stream-output.txt"
printf '0' >"$CALL_COUNT"
run_host streaming >"$STREAM_OUTPUT" &
stream_pid=$!
for _ in $(seq 1 20); do
  if grep -q streaming-line1 "$STREAM_OUTPUT" 2>/dev/null; then
    break
  fi
  sleep 0.05
done
grep -q streaming-line1 "$STREAM_OUTPUT" || {
  echo "expected streaming-line1 before host scrape completed" >&2
  kill "$stream_pid" 2>/dev/null || true
  wait "$stream_pid" 2>/dev/null || true
  exit 1
}
wait "$stream_pid"
grep -q streaming-line2 "$STREAM_OUTPUT" || {
  echo "expected streaming-line2 in host scrape output" >&2
  exit 1
}

COMPOSE_TTY_LOG="$TMP_DIR/compose-tty-default.log"
FAKE_COMPOSE="$TMP_DIR/fake-compose"
cat >"$FAKE_COMPOSE" <<'EOF'
#!/usr/bin/env bash
printf '%s\n' "$*" >>"${FAKE_COMPOSE_ARGS_LOG:?}"
printf 'run succeeded\n'
EOF
chmod +x "$FAKE_COMPOSE"

run_host_compose_capture "$ENV_FILE" "$FAKE_COMPOSE" "$COMPOSE_TTY_LOG" >/dev/null
grep -q ' run --rm ' "$COMPOSE_TTY_LOG" || {
  echo "expected default compose run to omit -T for live TTY allocation" >&2
  cat "$COMPOSE_TTY_LOG" >&2
  exit 1
}
grep -qE '(^|[[:space:]])-T([[:space:]]|$)' "$COMPOSE_TTY_LOG" && {
  echo "expected default compose run not to pass -T" >&2
  cat "$COMPOSE_TTY_LOG" >&2
  exit 1
}

COMPOSE_NOTTY_LOG="$TMP_DIR/compose-tty-off.log"
run_host_compose_capture "$ENV_FILE" "$FAKE_COMPOSE" "$COMPOSE_NOTTY_LOG" DCE_COMPOSE_TTY=0 >/dev/null
grep -qE '(^|[[:space:]])-T([[:space:]]|$)' "$COMPOSE_NOTTY_LOG" || {
  echo "expected DCE_COMPOSE_TTY=0 compose run to use -T" >&2
  cat "$COMPOSE_NOTTY_LOG" >&2
  exit 1
}

echo "run-discord-scrape-host smoke test passed"
