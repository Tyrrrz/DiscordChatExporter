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
all_args=( "$@" )
while (($#)); do
  case "$1" in
    --env-file)
      if [[ $# -ge 2 && -f "$2" ]]; then
        while IFS='=' read -r env_key env_value || [[ -n "$env_key" ]]; do
          [[ -z "$env_key" || "$env_key" =~ ^# ]] && continue
          env_key=${env_key#export }
          env_key=${env_key%%[[:space:]]*}
          printf -v "$env_key" '%s' "$env_value"
          export "$env_key"
        done <"$2"
      fi
      shift 2
      ;;
    *)
      shift
      ;;
  esac
done
printf 'env:DCE_CONTAINER_MEMORY=%s\n' "${DCE_CONTAINER_MEMORY:-}" >>"${FAKE_COMPOSE_ARGS_LOG:?}"
printf '%s\n' "${all_args[*]}" >>"${FAKE_COMPOSE_ARGS_LOG:?}"
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

MEM_ENV="$TMP_DIR/mem.env"
cat >"$MEM_ENV" <<EOF
DISCORD_TOKEN=dummy
DCE_CONTAINER_MEMORY=8g
EOF
COMPOSE_MEM_LOG="$TMP_DIR/compose-mem.log"
run_host_compose_capture "$MEM_ENV" "$FAKE_COMPOSE" "$COMPOSE_MEM_LOG" >/dev/null
grep -q 'env:DCE_CONTAINER_MEMORY=8g' "$COMPOSE_MEM_LOG" || {
  echo "expected DCE_CONTAINER_MEMORY=8g in compose env file passthrough" >&2
  cat "$COMPOSE_MEM_LOG" >&2
  exit 1
}

TARGET_MEM_CONFIG="$TMP_DIR/target-mem-config.json"
mkdir -p "$TMP_DIR/archive/demo"
cat >"$TARGET_MEM_CONFIG" <<EOF
{
  "archive_root": "$TMP_DIR/archive",
  "targets": [
    {
      "name": "demo",
      "output_dir": "$TMP_DIR/archive/demo",
      "container_memory": "4g"
    }
  ]
}
EOF
ENV_NO_MEM="$TMP_DIR/no-mem.env"
printf 'DISCORD_TOKEN=dummy\n' >"$ENV_NO_MEM"
COMPOSE_TARGET_MEM_LOG="$TMP_DIR/compose-target-mem.log"
env -u DCE_CONTAINER_MEMORY \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_COMPOSE_BIN="$FAKE_COMPOSE" \
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_ENV_FILE="$ENV_NO_MEM" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  FAKE_COMPOSE_ARGS_LOG="$COMPOSE_TARGET_MEM_LOG" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape \
  --config "$TARGET_MEM_CONFIG" --target demo >/dev/null
grep -q 'env:DCE_CONTAINER_MEMORY=4g' "$COMPOSE_TARGET_MEM_LOG" || {
  echo "expected target container_memory=4g in compose env when global unset" >&2
  cat "$COMPOSE_TARGET_MEM_LOG" >&2
  exit 1
}

ENV_OVERRIDE="$TMP_DIR/override-mem.env"
printf 'DISCORD_TOKEN=dummy\nDCE_CONTAINER_MEMORY=2g\n' >"$ENV_OVERRIDE"
COMPOSE_OVERRIDE_LOG="$TMP_DIR/compose-override-mem.log"
env -u DCE_CONTAINER_MEMORY \
  DCE_SKIP_SCRAPE_LOCK=1 \
  DCE_COMPOSE_BIN="$FAKE_COMPOSE" \
  DCE_REPO_ROOT="$REPO_ROOT" \
  DCE_ENV_FILE="$ENV_OVERRIDE" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  FAKE_COMPOSE_ARGS_LOG="$COMPOSE_OVERRIDE_LOG" \
  "$REPO_ROOT/scripts/run-discord-scrape-host.sh" scrape \
  --config "$TARGET_MEM_CONFIG" --target demo >/dev/null
grep -q 'env:DCE_CONTAINER_MEMORY=2g' "$COMPOSE_OVERRIDE_LOG" || {
  echo "expected scrape.env DCE_CONTAINER_MEMORY to override target config" >&2
  cat "$COMPOSE_OVERRIDE_LOG" >&2
  exit 1
}

echo "run-discord-scrape-host smoke test passed"
