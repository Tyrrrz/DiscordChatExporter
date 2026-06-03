#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_PRIMARY_CONFIG:-$REPO_ROOT/config/scrape-targets.json}"
CONTAINER_CONFIG="${DCE_CONTAINER_CONFIG:-/config/scrape-targets.json}"
HOST_RUNNER="$REPO_ROOT/scripts/run-discord-scrape-host.sh"
SNAPSHOT_DIR=""

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --target NAME [--config PATH] [--channel ID]
  $(basename "$0") --target NAME --snapshot-only --snapshot-file PATH [--config PATH]
  $(basename "$0") --compare-snapshots BEFORE.tsv AFTER.tsv

Record message counts for JSON archives under the target's output_dir,
run one incremental scrape, then assert:
  - archive file paths are unchanged (no parallel channels/ fallbacks)
  - message counts never shrink

  --channel ID  Limit scrape and snapshot/compare to channel ID (repeatable)

Requires valid Discord auth (scrape.env, exported DISCORD_TOKEN, or token file).
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

cleanup() {
  if [[ -n "${SNAPSHOT_DIR:-}" && -d "$SNAPSHOT_DIR" ]]; then
    rm -rf "$SNAPSHOT_DIR"
  fi
  return 0
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

target_output_dir() {
  local target_name=$1
  jq -r --arg name "$target_name" '
    .targets[]
    | select(.name == $name)
    | .output_dir
  ' "$CONFIG_PATH"
}

snapshot_channel_allowed() {
  local channel_id=$1
  shift
  local -a filter_ids=("$@")
  local id

  ((${#filter_ids[@]} == 0)) && return 0
  for id in "${filter_ids[@]}"; do
    [[ "$id" == "$channel_id" ]] && return 0
  done
  return 1
}

snapshot_archives() {
  local output_dir=$1
  local snapshot_file=$2
  shift 2
  local -a channel_filter=("$@")
  local file_path file_name channel_id count

  : >"$snapshot_file"

  [[ -d "$output_dir" ]] || die "Missing output_dir: $output_dir"

  while IFS= read -r -d '' file_path; do
    file_name=$(basename "$file_path")
    if [[ "$file_name" =~ \[([0-9]{16,22})\]\.json$ ]]; then
      channel_id=${BASH_REMATCH[1]}
      if ! snapshot_channel_allowed "$channel_id" "${channel_filter[@]}"; then
        continue
      fi
      if ! jq empty "$file_path" >/dev/null 2>&1; then
        printf 'WARN: skipping invalid JSON during snapshot: %s\n' "$file_path" >&2
        continue
      fi
      count=$(jq -r '(.messages | length) // 0' "$file_path")
      printf '%s\t%s\t%s\n' "$file_path" "$channel_id" "$count" >>"$snapshot_file"
    fi
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' ! -path '*/.dce-temp/*' -print0 2>/dev/null)
}

compare_snapshots() {
  local before_file=$1
  local after_file=$2
  local failures=0

  while IFS=$'\t' read -r path channel_id before_count; do
    [[ -n "$path" ]] || continue
    local after_line after_path after_count
    after_line=$(grep -F "$path"$'\t' "$after_file" || true)
    if [[ -z "$after_line" ]]; then
      printf 'FAIL: archive disappeared: %s\n' "$path" >&2
      failures=$((failures + 1))
      continue
    fi
    IFS=$'\t' read -r after_path _ after_count <<<"$after_line"
    if (( after_count < before_count )); then
      printf 'FAIL: message count shrank for %s (%s -> %s)\n' "$path" "$before_count" "$after_count" >&2
      failures=$((failures + 1))
      continue
    fi
    if (( after_count > before_count )); then
      printf 'OK: appended %s messages in %s\n' "$((after_count - before_count))" "$path"
    else
      printf 'OK: unchanged %s (%s messages)\n' "$path" "$before_count"
    fi
  done <"$before_file"

  while IFS=$'\t' read -r path channel_id after_count; do
    [[ -n "$path" ]] || continue
    if ! grep -Fq "$path"$'\t' "$before_file"; then
      printf 'FAIL: unexpected new archive path (not pre-existing): %s\n' "$path" >&2
      failures=$((failures + 1))
    fi
  done <"$after_file"

  if (( failures > 0 )); then
    die "$failures archive integrity check(s) failed."
  fi
}

main() {
  local target=""
  local snapshot_only=0
  local snapshot_file=""
  local compare_before=""
  local compare_after=""
  local -a channel_args=()
  local -a channel_ids=()

  trap cleanup EXIT

  while (($#)); do
    case "$1" in
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        target=$2
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --snapshot-only)
        snapshot_only=1
        shift
        ;;
      --snapshot-file)
        [[ $# -ge 2 ]] || die "Missing value for --snapshot-file."
        snapshot_file=$2
        shift 2
        ;;
      --compare-snapshots)
        [[ $# -ge 3 ]] || die "Missing paths for --compare-snapshots."
        compare_before=$2
        compare_after=$3
        shift 3
        ;;
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        channel_args+=(--channel "$2")
        channel_ids+=("$2")
        shift 2
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      *)
        die "Unknown option: $1"
        ;;
    esac
  done

  require_command jq

  if [[ -n "$compare_before" ]]; then
    [[ -f "$compare_before" ]] || die "Missing snapshot: $compare_before"
    [[ -f "$compare_after" ]] || die "Missing snapshot: $compare_after"
    compare_snapshots "$compare_before" "$compare_after"
    printf 'Snapshot comparison passed.\n'
    exit 0
  fi

  [[ -n "$target" ]] || die "--target is required."

  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  local output_dir
  output_dir=$(target_output_dir "$target")
  [[ -n "$output_dir" && "$output_dir" != "null" ]] || die "Unknown target: $target"

  if (( snapshot_only )); then
    [[ -n "$snapshot_file" ]] || die "--snapshot-file is required with --snapshot-only."
    snapshot_archives "$output_dir" "$snapshot_file" "${channel_ids[@]}"
    [[ -s "$snapshot_file" ]] || die "No seeded archives found under $output_dir for channel filter."
    printf 'Snapshot written: %s\n' "$snapshot_file"
    exit 0
  fi

  SNAPSHOT_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-prove-append.XXXXXX")
  local before_file="$SNAPSHOT_DIR/before.tsv"
  local after_file="$SNAPSHOT_DIR/after.tsv"

  snapshot_archives "$output_dir" "$before_file" "${channel_ids[@]}"
  [[ -s "$before_file" ]] || die "No seeded archives found under $output_dir for channel filter."

  if ((${#channel_ids[@]} > 0)); then
    printf 'Channel-scoped proof for %s channel(s).\n' "${#channel_ids[@]}"
  fi
  printf 'Running incremental scrape for target %s...\n' "$target"
  local container_config="$CONTAINER_CONFIG"
  case "$CONFIG_PATH" in
    "$REPO_ROOT/config/scrape-targets.json"|config/scrape-targets.json|./config/scrape-targets.json) ;;
    *) container_config="$CONFIG_PATH" ;;
  esac
  "$HOST_RUNNER" scrape --config "$container_config" --target "$target" "${channel_args[@]}"

  snapshot_archives "$output_dir" "$after_file" "${channel_ids[@]}"
  compare_snapshots "$before_file" "$after_file"
  printf 'Append-safe proof passed for target %s.\n' "$target"
}

main "$@"
