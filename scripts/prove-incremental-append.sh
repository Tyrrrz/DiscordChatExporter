#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_PRIMARY_CONFIG:-$REPO_ROOT/config/scrape-targets.json}"
HOST_RUNNER="$REPO_ROOT/scripts/run-discord-scrape-host.sh"
SNAPSHOT_DIR=""

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --target NAME [--config PATH]

Record message counts for every JSON archive under the target's output_dir,
run one incremental scrape, then assert:
  - archive file paths are unchanged (no parallel channels/ fallbacks)
  - message counts never shrink

Requires valid Discord auth (scrape.env, exported DISCORD_TOKEN, or token file).
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

cleanup() {
  [[ -n "$SNAPSHOT_DIR" && -d "$SNAPSHOT_DIR" ]] && rm -rf "$SNAPSHOT_DIR"
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

snapshot_archives() {
  local output_dir=$1
  local snapshot_file=$2
  local file_path file_name channel_id count

  : >"$snapshot_file"

  [[ -d "$output_dir" ]] || die "Missing output_dir: $output_dir"

  while IFS= read -r -d '' file_path; do
    file_name=$(basename "$file_path")
    if [[ "$file_name" =~ \[([0-9]{16,22})\]\.json$ ]]; then
      channel_id=${BASH_REMATCH[1]}
      count=$(jq -r '(.messages | length) // 0' "$file_path")
      printf '%s\t%s\t%s\n' "$file_path" "$channel_id" "$count" >>"$snapshot_file"
    fi
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' -print0 2>/dev/null)
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
      --help|-h)
        usage
        exit 0
        ;;
      *)
        die "Unknown option: $1"
        ;;
    esac
  done

  [[ -n "$target" ]] || die "--target is required."

  require_command jq
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  local output_dir
  output_dir=$(target_output_dir "$target")
  [[ -n "$output_dir" && "$output_dir" != "null" ]] || die "Unknown target: $target"

  SNAPSHOT_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-prove-append.XXXXXX")
  local before_file="$SNAPSHOT_DIR/before.tsv"
  local after_file="$SNAPSHOT_DIR/after.tsv"

  snapshot_archives "$output_dir" "$before_file"
  [[ -s "$before_file" ]] || die "No seeded archives found under $output_dir"

  printf 'Running incremental scrape for target %s...\n' "$target"
  "$HOST_RUNNER" scrape --config "$CONFIG_PATH" --target "$target"

  snapshot_archives "$output_dir" "$after_file"
  compare_snapshots "$before_file" "$after_file"
  printf 'Append-safe proof passed for target %s.\n' "$target"
}

main "$@"
