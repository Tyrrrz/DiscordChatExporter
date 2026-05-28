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
    },
    {
      "name": "partial-write",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/partial-write",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "concurrent-conflict",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/concurrent-conflict",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "idempotent",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/idempotent",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "cursor-max-id",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/cursor-max-id",
      "channel_ids": ["111"],
      "guild_ids": [],
      "guild_name_patterns": []
    },
    {
      "name": "bootstrap-map",
      "kind": "guild",
      "output_dir": "$ARCHIVE_ROOT/bootstrap-map",
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
    after=""
    while (($#)); do
      case "$1" in
        --output)
          output=$2
          shift 2
          ;;
        --after)
          after=$2
          if [[ -n "${FAKE_DCE_EXPECT_AFTER:-}" && "$after" != "${FAKE_DCE_EXPECT_AFTER}" ]]; then
            echo "unexpected --after value: $after (expected ${FAKE_DCE_EXPECT_AFTER})" >&2
            exit 1
          fi
          shift 2
          ;;
        --channel|--format)
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
      append-after-high-id) cp "$fixture_dir/append-after-high-id.json" "$output" ;;
      partial-write) cp "$fixture_dir/append-partial-write.json" "$output" ;;
      concurrent-conflict) cp "$fixture_dir/append-concurrent-conflict.json" "$output" ;;
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
  FAKE_DCE_EXPECT_AFTER="${FAKE_DCE_EXPECT_AFTER:-}" \
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

# U1: Test partial-write scenario (single message after merge)
mkdir -p "$ARCHIVE_ROOT/partial-write"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/partial-write/$DEFAULT_FILE_NAME"
run_wrapper partial-write partial-write
PARTIAL_DEST="$ARCHIVE_ROOT/partial-write/$DEFAULT_FILE_NAME"
[[ -f "$PARTIAL_DEST" ]] || { echo "expected partial-write archive missing" >&2; exit 1; }
[[ "$(jq -r '.messages | length' "$PARTIAL_DEST")" == "3" ]] || { echo "expected partial-write message count of 3 (2 existing + 1 new)" >&2; exit 1; }
[[ "$(jq -r '.messages[-1].id' "$PARTIAL_DEST")" == "4" ]] || { echo "expected last message id 4 after partial-write" >&2; exit 1; }
# Verify messages are sorted by timestamp and id
last_timestamp=$(jq -r '.messages[-1].timestamp' "$PARTIAL_DEST")
last_id=$(jq -r '.messages[-1].id' "$PARTIAL_DEST")
[[ "$last_timestamp" == "2026-01-04T00:00:00Z" ]] || { echo "expected last message timestamp 2026-01-04T00:00:00Z, got $last_timestamp" >&2; exit 1; }
[[ "$last_id" == "4" ]] || { echo "expected last message id 4, got $last_id" >&2; exit 1; }

# U1: Test concurrent-conflict scenario (overlapping messages deduplicated by id)
mkdir -p "$ARCHIVE_ROOT/concurrent-conflict"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/concurrent-conflict/$DEFAULT_FILE_NAME"
run_wrapper concurrent-conflict concurrent-conflict
CONFLICT_DEST="$ARCHIVE_ROOT/concurrent-conflict/$DEFAULT_FILE_NAME"
[[ -f "$CONFLICT_DEST" ]] || { echo "expected concurrent-conflict archive missing" >&2; exit 1; }
# Should have 4 unique messages (1, 2, 3, 4) - message 2 deduplicated, message 3 and 4 added
[[ "$(jq -r '.messages | length' "$CONFLICT_DEST")" == "4" ]] || { echo "expected concurrent-conflict message count of 4 (deduplicated by id)" >&2; exit 1; }
# Verify deduplication: message with id 2 should be the one from the concurrent-conflict fixture (higher precedence)
message_2_content=$(jq -r '.messages[] | select(.id=="2") | .content' "$CONFLICT_DEST")
[[ "$message_2_content" == "second (slightly modified)" ]] || { echo "expected message 2 to be from concurrent-conflict fixture (deduplicated), got: $message_2_content" >&2; exit 1; }

# U1: Test idempotency - merging the same incremental file twice should produce identical results
mkdir -p "$ARCHIVE_ROOT/idempotent"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/idempotent/$DEFAULT_FILE_NAME"
run_wrapper idempotent append
IDEMPOTENT_DEST="$ARCHIVE_ROOT/idempotent/$DEFAULT_FILE_NAME"
IDEMPOTENT_CHECKSUM_1=$(sha256sum "$IDEMPOTENT_DEST" | awk '{print $1}')
run_wrapper idempotent append
IDEMPOTENT_CHECKSUM_2=$(sha256sum "$IDEMPOTENT_DEST" | awk '{print $1}')
[[ "$IDEMPOTENT_CHECKSUM_1" == "$IDEMPOTENT_CHECKSUM_2" ]] || { echo "expected idempotent merge to produce identical results on repeat" >&2; exit 1; }

# U1: Verify message structure consistency - ensure all required fields present after merge
[[ "$(jq -r '.guild.id' "$DEST")" == "222" ]] || { echo "expected guild id to be preserved after merge" >&2; exit 1; }
[[ "$(jq -r '.channel.id' "$DEST")" == "111" ]] || { echo "expected channel id to be preserved after merge" >&2; exit 1; }
[[ "$(jq -r '.messages[0] | has("id") and has("timestamp") and has("content")' "$DEST")" == "true" ]] || { echo "expected message structure to be complete after merge" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/cursor-max-id"
cp "$FIXTURE_DIR/append-unordered-cursor.json" "$ARCHIVE_ROOT/cursor-max-id/$DEFAULT_FILE_NAME"
FAKE_DCE_EXPECT_AFTER=999 run_wrapper cursor-max-id append-after-high-id
CURSOR_DEST="$ARCHIVE_ROOT/cursor-max-id/$DEFAULT_FILE_NAME"
[[ "$(jq -r '.messages | length' "$CURSOR_DEST")" == "4" ]] || { echo "expected cursor-max-id archive to contain four messages" >&2; exit 1; }

mkdir -p "$ARCHIVE_ROOT/bootstrap-map"
cp "$FIXTURE_DIR/append-existing.json" "$ARCHIVE_ROOT/bootstrap-map/$DEFAULT_FILE_NAME"
[[ ! -f "$ARCHIVE_ROOT/bootstrap-map/.dce-meta/channel-map.json" ]] || { echo "bootstrap-map should start without channel map" >&2; exit 1; }
run_wrapper bootstrap-map append
BOOTSTRAP_DEST="$ARCHIVE_ROOT/bootstrap-map/$DEFAULT_FILE_NAME"
bootstrap_mapped_dest=$(jq -r '."111"' "$ARCHIVE_ROOT/bootstrap-map/.dce-meta/channel-map.json")
[[ "$bootstrap_mapped_dest" == "$BOOTSTRAP_DEST" ]] || { echo "expected bootstrap to register existing archive in channel map" >&2; exit 1; }
[[ "$(jq -r '.messages | length' "$BOOTSTRAP_DEST")" == "3" ]] || { echo "expected bootstrap-map archive to append in place" >&2; exit 1; }

# shellcheck disable=SC1091
source "$REPO_ROOT/scripts/run-discord-scrape.sh"
SHRINK_EXISTING="$TMP_DIR/shrink-existing.json"
SHRINK_MERGED="$TMP_DIR/shrink-merged.json"
cp "$FIXTURE_DIR/append-existing.json" "$SHRINK_EXISTING"
jq '.messages = [.messages[0]]' "$SHRINK_EXISTING" >"$SHRINK_MERGED"
if ( commit_merged_export "$SHRINK_EXISTING" "$SHRINK_MERGED" >/dev/null 2>&1 ); then
  echo "commit_merged_export should reject shrinking archives" >&2
  exit 1
fi
[[ "$(jq -r '.messages | length' "$SHRINK_EXISTING")" == "2" ]] || { echo "existing archive changed after rejected shrink merge" >&2; exit 1; }

echo "U1: append-only merge test coverage passed"

