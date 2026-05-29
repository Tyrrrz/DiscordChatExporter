#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
RUNNER="$REPO_ROOT/scripts/run-operator-validation.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-op-val-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
ENV_PATH="$TMP_DIR/scrape.env"
LOG_DIR="$TMP_DIR/logs"
FAKE_DOCKER="$TMP_DIR/docker"
PATH_BACKUP="$PATH"

cleanup() {
  export PATH="$PATH_BACKUP"
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$ARCHIVE_ROOT/demo" "$LOG_DIR"
printf '{"messages":[{"id":"1"}],"channel":{"id":"111111111111111111"}}\n' \
  >"$ARCHIVE_ROOT/demo/Guild - general [111111111111111111].json"

mkdir -p "$ARCHIVE_ROOT/demo2"
printf '{"messages":[{"id":"1"}],"channel":{"id":"222222222222222222"}}\n' \
  >"$ARCHIVE_ROOT/demo2/Guild - other [222222222222222222].json"

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "enabled": true
    },
    {
      "name": "demo2",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo2",
      "enabled": true
    }
  ]
}
JSON

printf 'DISCORD_TOKEN=dummy\n' >"$ENV_PATH"

cat >"$FAKE_DOCKER" <<'EOF'
#!/usr/bin/env bash
if [[ "${1:-}" == "compose" && "${2:-}" == "version" ]]; then
  exit 0
fi
exit 1
EOF
chmod +x "$FAKE_DOCKER"
export PATH="$TMP_DIR:$PATH_BACKUP"

DCE_REPO_ROOT="$REPO_ROOT" DCE_CONFIG_FILE="$CONFIG_PATH" DCE_ENV_FILE="$ENV_PATH" DCE_LOG_DIR="$LOG_DIR" \
  "$RUNNER" --dry-run --per-target --config "$CONFIG_PATH" --log-file "$LOG_DIR/validation.log"

grep -q 'Per-target summary: 2 succeeded, 0 failed' "$LOG_DIR/validation.log" || {
  printf 'ERROR: per-target summary missing from log\n' >&2
  exit 1
}

grep -q 'Operator validation finished successfully' "$LOG_DIR/validation.log" || {
  printf 'ERROR: validation log missing success marker\n' >&2
  exit 1
}

printf 'run-operator-validation-smoke: ok\n'
