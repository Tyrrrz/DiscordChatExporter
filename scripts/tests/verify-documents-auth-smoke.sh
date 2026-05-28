#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-verify-auth-smoke.XXXXXX")

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$TMP_DIR/good-server" "$TMP_DIR/bad-server"
printf '{"messages":[{"id":"1"}]}\n' >"$TMP_DIR/good-server/Guild - general [111111111111111111].json"

cat >"$TMP_DIR/config.json" <<JSON
{
  "archive_root": "$TMP_DIR",
  "targets": [
    {
      "name": "good-server",
      "kind": "guild",
      "output_dir": "$TMP_DIR/good-server",
      "channel_ids": [],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "bad-server",
      "enabled": false,
      "kind": "guild",
      "output_dir": "$TMP_DIR/bad-server",
      "channel_ids": [],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "missing-server",
      "kind": "guild",
      "output_dir": "$TMP_DIR/missing-server",
      "channel_ids": [],
      "guild_ids": [],
      "guild_name_patterns": []
    }
  ]
}
JSON

"$REPO_ROOT/scripts/verify-documents-archives.sh" --config "$TMP_DIR/config.json" >/dev/null && {
  echo "expected verify to fail when enabled target dir is missing" >&2
  exit 1
}

mkdir -p "$TMP_DIR/missing-server"
printf '{"messages":[{"id":"1"}]}\n' >"$TMP_DIR/missing-server/Guild - general [222222222222222222].json"
"$REPO_ROOT/scripts/verify-documents-archives.sh" --config "$TMP_DIR/config.json" >/dev/null

ENV_OUT="$TMP_DIR/scrape.env"
DISCORD_TOKEN=smoke-token \
  "$REPO_ROOT/scripts/setup-scrape-auth.sh" --env-file "$ENV_OUT"

grep -q '^DISCORD_TOKEN=smoke-token$' "$ENV_OUT" || {
  echo "expected setup-scrape-auth to write DISCORD_TOKEN" >&2
  exit 1
}
[[ "$(stat -c '%a' "$ENV_OUT")" == "600" ]] || {
  echo "expected scrape.env mode 600" >&2
  exit 1
}

DISCORD_TOKEN_FILE="$TMP_DIR/token.txt"
printf 'file-token\n' >"$DISCORD_TOKEN_FILE"
DISCORD_TOKEN_FILE="$DISCORD_TOKEN_FILE" \
  "$REPO_ROOT/scripts/setup-scrape-auth.sh" --env-file "$ENV_OUT" --force
grep -q "^DISCORD_TOKEN_FILE=$DISCORD_TOKEN_FILE\$" "$ENV_OUT" || {
  echo "expected setup-scrape-auth to write DISCORD_TOKEN_FILE" >&2
  exit 1
}

echo "verify-documents-auth-smoke: ok"
