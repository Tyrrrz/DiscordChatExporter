#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
FIXTURE_DIR="$REPO_ROOT/scripts/tests/test-fixtures"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-run-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
FAKE_CLI="$TMP_DIR/fake-cli.sh"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

cat >"$CONFIG_PATH" <<JSON
{
  "archive_root": "$ARCHIVE_ROOT",
  "defaults": {
    "include_threads": "all",
    "include_voice_channels": false
  },
  "targets": [
    {
      "name": "demo",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/demo",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    }
  ]
}
JSON

cat >"$FAKE_CLI" <<'EOF'
#!/usr/bin/env bash
set -Eeuo pipefail

mode=${FAKE_DCE_MODE:?}
fixture_dir=${FAKE_DCE_FIXTURE_DIR:?}
subcommand=${1:?}
shift || true

case "$subcommand" in
  export)
    output=""
    while (($#)); do
      case "$1" in
        --output)
          output=$2
          shift 2
          ;;
        --channel|--format|--after)
          shift 2
          ;;
        *)
          shift
          ;;
      esac
    done

    case "$mode" in
      initial) cp "$fixture_dir/append-existing.json" "$output" ;;
      append) cp "$fixture_dir/append-incremental.json" "$output" ;;
      wrong-channel) cp "$fixture_dir/wrong-channel.json" "$output" ;;
      *) echo "unexpected mode: $mode" >&2; exit 1 ;;
    esac
    ;;
  *)
    echo "unexpected subcommand: $subcommand" >&2
    exit 1
    ;;
esac
EOF
chmod +x "$FAKE_CLI"

run_wrapper() {
  DISCORD_TOKEN=dummy \
  DCE_CLI_BIN="$FAKE_CLI" \
  DCE_PRIMARY_CONFIG="$CONFIG_PATH" \
  DCE_FALLBACK_CONFIG="$CONFIG_PATH" \
  FAKE_DCE_FIXTURE_DIR="$FIXTURE_DIR" \
  FAKE_DCE_MODE="$1" \
  "$REPO_ROOT/scripts/run-discord-scrape.sh" scrape --target demo
}

run_wrapper initial

DEST="$ARCHIVE_ROOT/demo/channels/111.json"
[[ -f "$DEST" ]] || { echo "expected destination archive missing" >&2; exit 1; }
[[ "$(jq -r '.messages | length' "$DEST")" == "2" ]] || { echo "expected initial message count of 2" >&2; exit 1; }

run_wrapper append
[[ "$(jq -r '.messages | length' "$DEST")" == "3" ]] || { echo "expected appended message count of 3" >&2; exit 1; }
[[ "$(jq -r '.messages[-1].id' "$DEST")" == "3" ]] || { echo "expected last message id 3 after append" >&2; exit 1; }

before_checksum=$(sha256sum "$DEST" | awk '{print $1}')
if run_wrapper wrong-channel; then
  echo "wrong-channel fixture should have failed" >&2
  exit 1
fi
after_checksum=$(sha256sum "$DEST" | awk '{print $1}')
[[ "$before_checksum" == "$after_checksum" ]] || { echo "destination archive changed after failed wrong-channel run" >&2; exit 1; }

echo "run-discord-scrape smoke test passed"
