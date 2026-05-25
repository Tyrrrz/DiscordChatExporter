#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
FIXTURE_DIR="$REPO_ROOT/scripts/tests/test-fixtures"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-run-smoke.XXXXXX")
ARCHIVE_ROOT="$TMP_DIR/archive"
CONFIG_PATH="$TMP_DIR/config.json"
FAKE_CLI="$TMP_DIR/fake-cli.sh"
DEFAULT_FILE_NAME="Fixture Guild - Testing Grounds - fixture-room [111].json"

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
    },
    {
      "name": "seeded",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/seeded",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "duplicate",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/duplicate",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "invalid",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/invalid",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "mapped-outside-root",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/mapped-outside-root",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "seeded-wrong-channel",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/seeded-wrong-channel",
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
  local target_name=$1
  local mode=$2

  DISCORD_TOKEN=dummy \
  DCE_CLI_BIN="$FAKE_CLI" \
  DCE_PRIMARY_CONFIG="$CONFIG_PATH" \
  DCE_FALLBACK_CONFIG="$CONFIG_PATH" \
  FAKE_DCE_FIXTURE_DIR="$FIXTURE_DIR" \
  FAKE_DCE_MODE="$mode" \
  "$REPO_ROOT/scripts/run-discord-scrape.sh" scrape --target "$target_name"
}

run_wrapper demo initial

DEST="$ARCHIVE_ROOT/demo/$DEFAULT_FILE_NAME"
[[ -f "$DEST" ]] || { echo "expected destination archive missing" >&2; exit 1; }
[[ "$(jq -r '.messages | length' "$DEST")" == "2" ]] || { echo "expected initial message count of 2" >&2; exit 1; }
[[ ! -d "$ARCHIVE_ROOT/demo/channels" ]] || { echo "unexpected channels directory created for default fallback" >&2; exit 1; }

run_wrapper demo append
[[ "$(jq -r '.messages | length' "$DEST")" == "3" ]] || { echo "expected appended message count of 3" >&2; exit 1; }
[[ "$(jq -r '.messages[-1].id' "$DEST")" == "3" ]] || { echo "expected last message id 3 after append" >&2; exit 1; }
mapped_dest=$(jq -r '."111"' "$ARCHIVE_ROOT/demo/.dce-meta/channel-map.json")
[[ "$mapped_dest" == "$DEST" ]] || { echo "expected channel map to point to human-readable destination" >&2; exit 1; }

before_checksum=$(sha256sum "$DEST" | awk '{print $1}')
if run_wrapper demo wrong-channel; then
  echo "wrong-channel fixture should have failed" >&2
  exit 1
fi
after_checksum=$(sha256sum "$DEST" | awk '{print $1}')
[[ "$before_checksum" == "$after_checksum" ]] || { echo "destination archive changed after failed wrong-channel run" >&2; exit 1; }
[[ ! -e "$ARCHIVE_ROOT/demo/channels/111.json" ]] || { echo "unexpected legacy fallback file created" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/seeded"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/seeded/$DEFAULT_FILE_NAME"

run_wrapper seeded append
SEEDED_DEST="$ARCHIVE_ROOT/seeded/$DEFAULT_FILE_NAME"
[[ -f "$SEEDED_DEST" ]] || { echo "expected seeded archive missing" >&2; exit 1; }
[[ "$(jq -r '.messages | length' "$SEEDED_DEST")" == "3" ]] || { echo "expected seeded archive to be updated in place" >&2; exit 1; }
seeded_mapped_dest=$(jq -r '."111"' "$ARCHIVE_ROOT/seeded/.dce-meta/channel-map.json")
[[ "$seeded_mapped_dest" == "$SEEDED_DEST" ]] || { echo "expected seeded channel map to point to existing archive" >&2; exit 1; }
[[ ! -e "$ARCHIVE_ROOT/seeded/channels/111.json" ]] || { echo "unexpected fallback file created for seeded archive" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/duplicate"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/duplicate/$DEFAULT_FILE_NAME"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/duplicate/Fixture Guild - Another Path [111].json"
if run_wrapper duplicate append; then
  echo "duplicate existing matches should have failed" >&2
  exit 1
fi

mkdir -p "$ARCHIVE_ROOT/invalid"
printf 'not-json\n' >"$ARCHIVE_ROOT/invalid/$DEFAULT_FILE_NAME"
if run_wrapper invalid append; then
  echo "invalid existing archive should have failed" >&2
  exit 1
fi
[[ ! -e "$ARCHIVE_ROOT/invalid/channels/111.json" ]] || { echo "unexpected fallback file created for invalid archive" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/mapped-outside-root/.dce-meta"
printf '{\"111\":\"%s\"}\n' "$ARCHIVE_ROOT/mapped-outside-root/../outside.json" >"$ARCHIVE_ROOT/mapped-outside-root/.dce-meta/channel-map.json"
if run_wrapper mapped-outside-root append; then
  echo "mapped path outside target root should have failed" >&2
  exit 1
fi
[[ ! -e "$ARCHIVE_ROOT/outside.json" ]] || { echo "unexpected outside-root file created from mapped path" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/seeded-wrong-channel"
cp "$FIXTURE_DIR/wrong-channel.json" "$ARCHIVE_ROOT/seeded-wrong-channel/$DEFAULT_FILE_NAME"
if run_wrapper seeded-wrong-channel append; then
  echo "seeded archive with wrong embedded channel should have failed" >&2
  exit 1
fi
[[ ! -e "$ARCHIVE_ROOT/seeded-wrong-channel/channels/111.json" ]] || { echo "unexpected fallback file created for wrong-channel seeded archive" >&2; exit 1; }

echo "run-discord-scrape smoke test passed"
