#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
VERIFY="$REPO_ROOT/scripts/verify-operator-ready.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-op-ready-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
ENV_PATH="$TMP_DIR/scrape.env"
FAKE_DOCKER="$TMP_DIR/docker"
PATH_BACKUP="$PATH"

cleanup() {
  export PATH="$PATH_BACKUP"
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE_ROOT/demo"
printf '{"messages":[{"id":"1"}],"channel":{"id":"111111111111111111"}}\n' \
  >"$ARCHIVE_ROOT/demo/Guild - general [111111111111111111].json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "container_memory": "4g",
      "enabled": true
    }
  ]
}
JSON

printf 'DISCORD_TOKEN=dummy\n' >"$ENV_PATH"

mem_output=$(
  DCE_MIN_FREE_MB=0 DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$CONFIG_PATH" DCE_ENV_FILE="$ENV_PATH" \
    "$VERIFY" --config "$CONFIG_PATH" 2>&1
)
grep -q 'Operator ready' <<<"$mem_output" || {
  printf 'ERROR: verify-operator-ready failed\n' >&2
  printf '%s\n' "$mem_output" >&2
  exit 1
}
grep -q 'target memory: demo → 4g' <<<"$mem_output" || {
  printf 'ERROR: expected per-target memory hint in verify output\n' >&2
  printf '%s\n' "$mem_output" >&2
  exit 1
}
grep -qE 'demo[[:space:]].*4g' <<<"$mem_output" || {
  printf 'ERROR: expected MEM column for demo target\n' >&2
  printf '%s\n' "$mem_output" >&2
  exit 1
}

printf 'DISCORD_TOKEN=dummy\nDCE_CONTAINER_MEMORY=8g\n' >"$ENV_PATH"
mem_output=$(
  DCE_MIN_FREE_MB=0 DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$CONFIG_PATH" DCE_ENV_FILE="$ENV_PATH" \
    "$VERIFY" --config "$CONFIG_PATH" 2>&1
)
grep -q 'container memory: 8g' <<<"$mem_output" || {
  printf 'ERROR: expected container memory line in verify output\n' >&2
  printf '%s\n' "$mem_output" >&2
  exit 1
}
if grep -q 'target memory: demo → 4g' <<<"$mem_output"; then
  printf 'ERROR: per-target memory hint should be hidden when global cap is set\n' >&2
  exit 1
fi

cat >"$FAKE_DOCKER" <<'EOF'
#!/usr/bin/env bash
if [[ "${1:-}" == "compose" && "${2:-}" == "version" ]]; then
  exit 0
fi
exit 1
EOF
chmod +x "$FAKE_DOCKER"
export PATH="$TMP_DIR:$PATH_BACKUP"

DCE_MIN_FREE_MB=0 DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$CONFIG_PATH" DCE_ENV_FILE="$ENV_PATH" \
  "$VERIFY" --config "$CONFIG_PATH"

if DCE_MIN_FREE_MB=0 DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$CONFIG_PATH" DCE_ENV_FILE="$ENV_PATH" \
  "$VERIFY" --config "$CONFIG_PATH" --preflight demo 2>/dev/null; then
  printf 'ERROR: preflight should fail without real container/token\n' >&2
  exit 1
fi

KOTOR_ARCHIVE="$TMP_DIR/archive/kotor"
mkdir -p "$KOTOR_ARCHIVE"
printf '{"messages":[{"id":"1"}],"channel":{"id":"221726893064454144"}}\n' \
  >"$KOTOR_ARCHIVE/Guild - yes_general [221726893064454144].json"
KOTOR_CONFIG="$TMP_DIR/kotor-config.json"
cat >"$KOTOR_CONFIG" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "KotOR_discord_msgs",
      "kind": "guild",
      "output_dir": "$KOTOR_ARCHIVE",
      "enabled": true
    }
  ]
}
JSON

kotor_output=$(
  DCE_MIN_FREE_MB=0 DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$KOTOR_CONFIG" DCE_ENV_FILE="$ENV_PATH" \
    "$VERIFY" --config "$KOTOR_CONFIG" 2>&1
)
grep -q 'run-kotor-yes-general-catchup.sh' <<<"$kotor_output" || {
  printf 'ERROR: verify-operator-ready missing KotOR catch-up hint\n' >&2
  printf '%s\n' "$kotor_output" >&2
  exit 1
}

printf 'verify-operator-ready-smoke: ok\n'
