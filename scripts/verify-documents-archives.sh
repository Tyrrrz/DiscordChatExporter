#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_PRIMARY_CONFIG:-$REPO_ROOT/config/scrape-targets.json}"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--config PATH]

Verify enabled scrape targets against on-disk ~/Documents archive folders.
Reports JSON export counts, archive-seeded channel IDs, and channel-map coverage.
Exits non-zero when an enabled target's output_dir is missing or has zero JSON exports.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

count_archive_json() {
  local output_dir=$1
  find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' ! -path '*/.dce-temp/*' 2>/dev/null | wc -l | tr -d ' '
}

count_seeded_channel_ids() {
  local output_dir=$1
  local file_path file_name

  [[ -d "$output_dir" ]] || return 0

  while IFS= read -r -d '' file_path; do
    file_name=$(basename "$file_path")
    if [[ "$file_name" =~ \[([0-9]{16,22})\]\.json$ ]]; then
      printf '%s\n' "${BASH_REMATCH[1]}"
    fi
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' ! -path '*/.dce-temp/*' -print0 2>/dev/null) | sort -u | wc -l | tr -d ' '
}

count_channel_map_entries() {
  local map_file=$1
  [[ -f "$map_file" ]] || { printf '0'; return 0; }
  jq -r 'keys | length' "$map_file"
}

main() {
  while (($#)); do
    case "$1" in
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

  require_command jq
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  local archive_root failures=0
  archive_root=$(jq -r '.archive_root // empty' "$CONFIG_PATH")
  [[ -n "$archive_root" ]] || die "Config is missing archive_root."

  printf 'Archive root: %s\n\n' "$archive_root"
  printf '%-28s %-40s %8s %8s %8s %6s %s\n' "TARGET" "OUTPUT_DIR" "JSON" "SEEDED" "MAP" "MEM" "STATUS"
  printf '%-28s %-40s %8s %8s %8s %6s %s\n' "------" "----------" "----" "------" "-----" "---" "------"

  local target_json name output_dir enabled json_count seeded_count map_count map_file mem status
  while IFS= read -r target_json; do
    name=$(jq -r '.name' <<<"$target_json")
    output_dir=$(jq -r '.output_dir' <<<"$target_json")
    enabled=$(jq -r 'if has("enabled") then .enabled else true end' <<<"$target_json")

    if [[ "$enabled" == "false" ]]; then
      mem=$(jq -r '.container_memory // "-"' <<<"$target_json")
      printf '%-28s %-40s %8s %8s %8s %6s %s\n' "$name" "$output_dir" "-" "-" "-" "$mem" "disabled"
      continue
    fi

    json_count=0
    seeded_count=0
    map_count=0
    status="ok"

    if [[ ! -d "$output_dir" ]]; then
      status="missing output_dir"
      failures=$((failures + 1))
    else
      json_count=$(count_archive_json "$output_dir")
      seeded_count=$(count_seeded_channel_ids "$output_dir")
      map_file="$output_dir/.dce-meta/channel-map.json"
      map_count=$(count_channel_map_entries "$map_file")
      if (( json_count == 0 )); then
        status="no json archives"
        failures=$((failures + 1))
      elif (( seeded_count == 0 )); then
        status="no seeded channel ids"
        failures=$((failures + 1))
      elif (( map_count == 0 )); then
        status="ok (map will bootstrap on first run)"
      fi
    fi

    mem=$(jq -r '.container_memory // "-"' <<<"$target_json")
    [[ -n "$mem" && "$mem" != "null" ]] || mem="-"

    printf '%-28s %-40s %8s %8s %8s %6s %s\n' "$name" "$output_dir" "$json_count" "$seeded_count" "$map_count" "$mem" "$status"
  done < <(jq -c '.targets[]' "$CONFIG_PATH")

  printf '\n'
  if (( failures > 0 )); then
    die "$failures enabled target(s) failed archive verification."
  fi

  printf 'All enabled targets have archive directories with seeded channel exports.\n'
}

main "$@"
