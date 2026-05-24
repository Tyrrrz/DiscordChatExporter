#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_ENV=$(mktemp "${TMPDIR:-/tmp}/dce-container-smoke.XXXXXX.env")

cleanup() {
  rm -f "$TMP_ENV"
}
trap cleanup EXIT

cat >"$TMP_ENV" <<EOF
DISCORD_TOKEN=dummy
DCE_UID=$(id -u)
DCE_GID=$(id -g)
TZ=UTC
EOF

cd "$REPO_ROOT"
docker compose --env-file "$TMP_ENV" build
docker compose --env-file "$TMP_ENV" run --rm discord-scraper help >/dev/null
docker compose --env-file "$TMP_ENV" run --rm discord-scraper list-targets >/dev/null

echo "container smoke test passed"
