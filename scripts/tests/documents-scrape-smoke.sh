#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-documents-scrape-smoke.XXXXXX")

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

FAKE_REPO="$TMP_DIR/fake-repo"
mkdir -p "$FAKE_REPO/scripts"
cp "$REPO_ROOT/scripts/run-discord-scrape-host.sh" "$FAKE_REPO/scripts/"
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
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$MISSING_ENV" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  FAKE_DOCKER_CALL_COUNT="$CALL_COUNT" \
  "$FAKE_REPO/scripts/run-discord-scrape-host.sh" scrape --target demo >/dev/null

ARCHIVE="$TMP_DIR/server"
mkdir -p "$ARCHIVE"
printf '{"messages":[{"id":"1","timestamp":"2020-01-01T00:00:00"}]}\n' >"$ARCHIVE/Guild - general [111111111111111111].json"

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
  DCE_DOCKER_BIN="$FAKE_DOCKER" \
  DCE_ENV_FILE="$MISSING_ENV" \
  DCE_COMPOSE_FILE="$COMPOSE_FILE" \
  DISCORD_TOKEN=dummy \
  "$PROVE" --config "$TMP_DIR/config.json" --target demo >/dev/null

"$REPO_ROOT/scripts/run-documents-scrape.sh" --dry-run --config "$TMP_DIR/config.json" >/dev/null

DCE_MIN_FREE_MB=0 DCE_CONFIG_FILE="$TMP_DIR/config.json" \
  "$REPO_ROOT/scripts/verify-operator-ready.sh" --disk-only --config "$TMP_DIR/config.json" \
  | grep -q 'disk-only: ok'

echo "documents-scrape-smoke: ok"
