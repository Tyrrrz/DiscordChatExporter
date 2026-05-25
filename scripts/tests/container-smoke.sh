#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_ENV=$(mktemp "${TMPDIR:-/tmp}/dce-container-smoke.XXXXXX.env")
TMP_PODMAN_ENV=$(mktemp "${TMPDIR:-/tmp}/dce-container-smoke.XXXXXX.podman.env")
ARCHIVE_ROOT=$(jq -r '.archive_root' "$REPO_ROOT/config/scrape-targets.json")
WRITE_TEST_DIR="$ARCHIVE_ROOT/.dce-container-smoke-$$"

cleanup() {
  rm -f "$TMP_ENV"
  rm -f "$TMP_PODMAN_ENV"
  rm -rf "$WRITE_TEST_DIR"
}
trap cleanup EXIT

cat >"$TMP_ENV" <<EOF
DISCORD_TOKEN=dummy
DCE_UID=$(id -u)
DCE_GID=$(id -g)
TZ=UTC
EOF

cp "$TMP_ENV" "$TMP_PODMAN_ENV"
printf 'DCE_USERNS_MODE=keep-id\n' >>"$TMP_PODMAN_ENV"

cd "$REPO_ROOT"
docker compose --env-file "$TMP_ENV" build
docker compose --env-file "$TMP_ENV" run --rm discord-scraper help >/dev/null
docker compose --env-file "$TMP_ENV" run --rm discord-scraper list-targets >/dev/null

if docker version 2>&1 | grep -qi podman || docker info 2>&1 | grep -qi podman; then
  mkdir -p "$WRITE_TEST_DIR"
  docker compose --env-file "$TMP_PODMAN_ENV" run -T --rm --entrypoint /bin/sh discord-scraper -lc "mkdir -p '$WRITE_TEST_DIR/from-container' && rmdir '$WRITE_TEST_DIR/from-container'" >/dev/null
fi

echo "container smoke test passed"
