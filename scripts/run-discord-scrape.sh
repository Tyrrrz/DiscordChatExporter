#!/usr/bin/env bash

set -Eeuo pipefail

CLI_BIN="${DCE_CLI_BIN:-/opt/app/DiscordChatExporter.Cli}"
PRIMARY_CONFIG="${DCE_PRIMARY_CONFIG:-/config/scrape-targets.json}"
FALLBACK_CONFIG="${DCE_FALLBACK_CONFIG:-/opt/dce-config/scrape-targets.json}"

OVERRIDE_GUILDS=()
OVERRIDE_CHANNELS=()
CACHE_ROOT=""

usage() {
  cat <<'EOF'
Usage:
  run-discord-scrape.sh scrape [options]
  run-discord-scrape.sh preflight [options]
  run-discord-scrape.sh list-targets [--config PATH]
  run-discord-scrape.sh help
  run-discord-scrape.sh <DiscordChatExporter CLI args...>

Subcommands:
  scrape        Incrementally export channels into append-only JSON files.
  preflight     Validate token/config/target resolution without writing archives.
  list-targets  Print configured targets from the scrape config.
  help          Show this help text.

Options:
  --config PATH     Config file path inside the container.
  --target NAME     Restrict the run to one configured target. Repeatable.
  --guild ID        Narrow a selected target to one of its allowed guild IDs. Repeatable.
  --channel ID      Narrow a selected target to one of its allowed channel IDs. Repeatable.

Notes:
  * DISCORD_TOKEN must be provided via environment variables.
  * Channel exports are always stored as JSON because the append-only merge flow depends on it.
  * Unknown subcommands are passed through to the raw DiscordChatExporter CLI.
EOF
}

timestamp() {
  date -u +"%Y-%m-%dT%H:%M:%SZ"
}

log() {
  printf '[%s] %s\n' "$(timestamp)" "$*" >&2
}

die() {
  log "ERROR: $*"
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

require_file() {
  [[ -f "$1" ]] || die "Required file not found: $1"
}

default_config_path() {
  if [[ -f "$PRIMARY_CONFIG" ]]; then
    printf '%s\n' "$PRIMARY_CONFIG"
  else
    printf '%s\n' "$FALLBACK_CONFIG"
  fi
}

normalize_name() {
  printf '%s' "$1" | tr '[:upper:]' '[:lower:]' | tr -cd '[:alnum:]'
}

escape_file_name_component() {
  printf '%s' "$1" \
    | tr '\r\n' '  ' \
    | sed -E 's#[/\\]+#_#g; s/[[:cntrl:]]+/ /g; s/[[:space:]]+/ /g; s/^ //; s/ $//'
}

join_name_parts() {
  local result=""
  local part

  for part in "$@"; do
    [[ -n "$part" ]] || continue
    if [[ -n "$result" ]]; then
      result+=" - "
    fi
    result+="$part"
  done

  printf '%s\n' "$result"
}

json_array_from_args() {
  jq -cn '$ARGS.positional' --args "$@"
}

contains_value() {
  local needle=$1
  shift
  local value

  for value in "$@"; do
    [[ "$value" == "$needle" ]] && return 0
  done

  return 1
}

assert_subset() {
  local label=$1
  local -n requested_ref=$2
  local -n allowed_ref=$3
  local value

  for value in "${requested_ref[@]}"; do
    contains_value "$value" "${allowed_ref[@]}" || die "$label '$value' is outside the selected target's allowed scope."
  done
}

path_is_within_root() {
  local root=$1
  local path=$2

  [[ "$path" == /* ]] || return 1

  case "$path" in
    *"/../"*|../*|*/..|..|*"/./"*|./*|*/.)
      return 1
      ;;
  esac

  case "$path" in
    "$root"|"${root}/"*)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

config_archive_root() {
  local config_path=$1
  jq -r '.archive_root // empty' "$config_path"
}

validate_config_contract() {
  local config_path=$1
  local archive_root output_dir name kind
  local -a duplicate_names duplicate_dirs

  require_file "$config_path"
  jq empty "$config_path" >/dev/null 2>&1 || die "Invalid JSON config: $config_path"

  archive_root=$(config_archive_root "$config_path")
  [[ -n "$archive_root" ]] || die "Config is missing top-level archive_root."
  [[ "$archive_root" == /* ]] || die "archive_root must be an absolute path."

  jq -e '.targets | type == "array" and length > 0' "$config_path" >/dev/null \
    || die "Config must define at least one target."

  mapfile -t duplicate_names < <(jq -r '.targets[].name' "$config_path" | sort | uniq -d)
  (( ${#duplicate_names[@]} == 0 )) || die "Duplicate target names found: ${duplicate_names[*]}"

  mapfile -t duplicate_dirs < <(jq -r '.targets[].output_dir' "$config_path" | sort | uniq -d)
  (( ${#duplicate_dirs[@]} == 0 )) || die "Duplicate target output directories found: ${duplicate_dirs[*]}"

  while IFS=$'\t' read -r name kind output_dir; do
    [[ -n "$name" ]] || die "Every target must have a name."
    [[ -n "$output_dir" ]] || die "Target '$name' is missing output_dir."
    [[ "$kind" == "guild" || "$kind" == "dms" ]] || die "Target '$name' has unsupported kind '$kind'."
    path_is_within_root "$archive_root" "$output_dir" \
      || die "Target '$name' output_dir '$output_dir' is outside archive_root '$archive_root'."
  done < <(jq -r '.targets[] | [.name, (.kind // "guild"), .output_dir] | @tsv' "$config_path")
}

load_archive_seed_channel_ids() {
  local output_dir=$1
  local file_path file_name channel_id

  [[ -d "$output_dir" ]] || return 0

  while IFS= read -r -d '' file_path; do
    file_name=$(basename "$file_path")
    if [[ "$file_name" =~ \[([0-9]{16,22})\]\.json$ ]]; then
      channel_id="${BASH_REMATCH[1]}"
      printf '%s\n' "$channel_id"
    fi
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' -print0)
}

bootstrap_channel_map_from_archives() {
  local output_dir=$1
  local map_file file_path file_name channel_id mapped_path embedded_channel_id bootstrapped=0

  [[ -d "$output_dir" ]] || return 0

  map_file=$(get_channel_map_path "$output_dir")
  ensure_json_file "$map_file"

  while IFS= read -r -d '' file_path; do
    file_name=$(basename "$file_path")
    if [[ ! "$file_name" =~ \[([0-9]{16,22})\]\.json$ ]]; then
      continue
    fi

    channel_id="${BASH_REMATCH[1]}"
    mapped_path=$(jq -r --arg channel_id "$channel_id" '.[$channel_id] // empty' "$map_file")
    if [[ -n "$mapped_path" ]]; then
      continue
    fi

    path_is_within_root "$output_dir" "$file_path" || continue
    jq empty "$file_path" >/dev/null 2>&1 || continue

    embedded_channel_id=$(jq -r '.channel.id // empty' "$file_path")
    if [[ -n "$embedded_channel_id" && "$embedded_channel_id" != "$channel_id" ]]; then
      log "Skipping bootstrap for '$file_path': filename channel id $channel_id does not match export metadata ($embedded_channel_id)."
      continue
    fi

    update_channel_map "$map_file" "$channel_id" "$file_path"
    bootstrapped=$((bootstrapped + 1))
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' -print0)

  if (( bootstrapped > 0 )); then
    log "Bootstrapped $bootstrapped channel map entries from existing archives under $output_dir."
  fi
}

parse_two_column_listing() {
  local line id name

  while IFS= read -r line; do
    if [[ "$line" =~ ^([0-9]{16,22})[[:space:]]+\|[[:space:]]+(.+)$ ]]; then
      id="${BASH_REMATCH[1]}"
      name="${BASH_REMATCH[2]}"
      printf '%s\t%s\n' "$id" "$name"
    fi
  done
}

parse_channel_listing() {
  local line id

  while IFS= read -r line; do
    if [[ "$line" =~ ^[[:space:]]*\*?[[:space:]]*([0-9]{16,22})[[:space:]]+\|[[:space:]]+ ]]; then
      id="${BASH_REMATCH[1]}"
      printf '%s\n' "$id"
    fi
  done
}

ensure_json_file() {
  local file_path=$1
  mkdir -p "$(dirname "$file_path")"

  if [[ ! -f "$file_path" ]]; then
    printf '{}\n' >"$file_path"
    chmod 644 "$file_path" 2>/dev/null || true
  fi
}

update_channel_map() {
  local map_file=$1
  local channel_id=$2
  local destination_path=$3
  local temp_file

  mkdir -p "$(dirname "$map_file")"
  temp_file=$(mktemp "$(dirname "$map_file")/channel-map.XXXXXX")
  jq --arg channel_id "$channel_id" --arg destination_path "$destination_path" \
    '.[$channel_id] = $destination_path' \
    "$map_file" >"$temp_file"
  mv "$temp_file" "$map_file"
  chmod 644 "$map_file" 2>/dev/null || true
}

get_channel_map_path() {
  local output_dir=$1
  printf '%s/.dce-meta/channel-map.json' "$output_dir"
}

default_destination_path_from_export() {
  local output_dir=$1
  local export_path=$2
  local guild_name category_name channel_name channel_id
  local escaped_guild_name escaped_category_name escaped_channel_name base_name

  guild_name=$(jq -r '.guild.name // empty' "$export_path")
  category_name=$(jq -r '.channel.category // empty' "$export_path")
  channel_name=$(jq -r '.channel.name // empty' "$export_path")
  channel_id=$(jq -r '.channel.id // empty' "$export_path")

  [[ -n "$channel_id" ]] || die "Export '$export_path' is missing channel.id metadata."

  escaped_guild_name=$(escape_file_name_component "$guild_name")
  escaped_category_name=$(escape_file_name_component "$category_name")
  escaped_channel_name=$(escape_file_name_component "$channel_name")

  base_name=$(join_name_parts "$escaped_guild_name" "$escaped_category_name" "$escaped_channel_name")
  [[ -n "$base_name" ]] || base_name="channel"

  printf '%s/%s [%s].json\n' "$output_dir" "$base_name" "$channel_id"
}

resolve_destination_path() {
  local output_dir=$1
  local channel_id=$2
  local export_path=${3:-}
  local map_file mapped_path
  local -a existing_candidates

  mkdir -p "$output_dir/.dce-meta"
  map_file=$(get_channel_map_path "$output_dir")
  ensure_json_file "$map_file"

  mapped_path=$(jq -r --arg channel_id "$channel_id" '.[$channel_id] // empty' "$map_file")
  if [[ -n "$mapped_path" ]]; then
    path_is_within_root "$output_dir" "$mapped_path" \
      || die "Mapped destination '$mapped_path' for channel $channel_id is outside target root '$output_dir'."
    printf '%s\n' "$mapped_path"
    return 0
  fi

  mapfile -t existing_candidates < <(
    find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' -print \
      | grep -F "[$channel_id].json" || true
  )

  if (( ${#existing_candidates[@]} > 1 )); then
    die "Found multiple existing JSON exports for channel $channel_id under $output_dir; add an explicit mapping in $(get_channel_map_path "$output_dir")."
  fi

  if (( ${#existing_candidates[@]} == 1 )); then
    jq empty "${existing_candidates[0]}" >/dev/null 2>&1 \
      || die "Existing export is not valid JSON: ${existing_candidates[0]}"
    assert_export_channel_identity "${existing_candidates[0]}" "$channel_id"
    update_channel_map "$map_file" "$channel_id" "${existing_candidates[0]}"
    printf '%s\n' "${existing_candidates[0]}"
    return 0
  fi

  [[ -n "$export_path" ]] || return 0

  mapped_path=$(default_destination_path_from_export "$output_dir" "$export_path")
  path_is_within_root "$output_dir" "$mapped_path" \
    || die "Derived destination '$mapped_path' for channel $channel_id is outside target root '$output_dir'."
  update_channel_map "$map_file" "$channel_id" "$mapped_path"
  printf '%s\n' "$mapped_path"
}

channel_id_from_export() {
  local export_path=$1
  jq -r '.channel.id // empty' "$export_path"
}

assert_export_channel_identity() {
  local export_path=$1
  local expected_channel_id=$2
  local actual_channel_id

  actual_channel_id=$(channel_id_from_export "$export_path")
  [[ -n "$actual_channel_id" ]] || die "Export '$export_path' is missing channel.id metadata."
  [[ "$actual_channel_id" == "$expected_channel_id" ]] \
    || die "Export '$export_path' belongs to channel '$actual_channel_id', expected '$expected_channel_id'."
}

last_message_id() {
  local export_path=$1

  [[ -f "$export_path" ]] || return 0
  jq -r '
    (.messages // [])
    | if length == 0 then empty else (
        sort_by(
          .id as $id
          | ($id | tostring) as $s
          | (22 - ($s | length)) as $pad
          | if $pad > 0 then ("0" * $pad) + $s else $s end
        )
        | last
        | .id
      ) end
  ' "$export_path"
}

message_count() {
  local export_path=$1
  jq -r '(.messages | length) // 0' "$export_path"
}

is_skippable_channel_export_failure() {
  local log_file=$1
  grep -qiE \
    "failed: forbidden|failed: not found|Missing Access|403 Forbidden|404 Not Found|Cannot read message history|No space left on device|SQLITE_FULL|ENOSPC|disk full|not enough space" \
    "$log_file"
}

export_channel_incremental() {
  local channel_id=$1
  local temp_export=$2
  local after_id=$3
  local -a export_command
  local export_log export_status=0

  export_command=("$CLI_BIN" export --channel "$channel_id" --format Json --output "$temp_export")
  if [[ -n "$after_id" ]]; then
    export_command+=(--after "$after_id")
  fi

  export_log=$(mktemp "${TMPDIR:-/tmp}/dce-export.${channel_id}.XXXXXX")
  set +e
  "${export_command[@]}" >"$export_log" 2>&1
  export_status=$?
  set -e

  if (( export_status == 0 )); then
    rm -f "$export_log"
    return 0
  fi

  if is_skippable_channel_export_failure "$export_log"; then
    log "Skipping channel $channel_id (inaccessible or non-fatal export error)."
    cat "$export_log" >&2
    rm -f "$export_log"
    return 2
  fi

  cat "$export_log" >&2
  rm -f "$export_log"
  return 1
}

commit_merged_export() {
  local destination_path=$1
  local merged_path=$2
  local before_count after_count atomic_path

  before_count=$(message_count "$destination_path")
  after_count=$(message_count "$merged_path")
  if (( after_count < before_count )); then
    die "Merge would shrink archive '$destination_path' ($before_count -> $after_count messages). Existing file was not modified."
  fi

  atomic_path=$(mktemp -p "$(dirname "$destination_path")" ".$(basename "$destination_path").dce-replace.XXXXXX")
  cp "$merged_path" "$atomic_path"
  jq empty "$atomic_path" >/dev/null 2>&1 || die "Merged export is not valid JSON: $atomic_path"
  assert_export_channel_identity "$atomic_path" "$(channel_id_from_export "$destination_path")"
  mv -f "$atomic_path" "$destination_path"
}

merge_exports() {
  local existing_path=$1
  local incremental_path=$2
  local merged_path=$3

  jq -s '
    .[0] as $existing
    | .[1] as $incremental
    | ($existing + $incremental)
    | .messages = (
        reduce (($existing.messages // []) + ($incremental.messages // []))[] as $message
          ({};
            .[$message.id] = $message
          )
        | to_entries
        | map(.value)
        | sort_by(.timestamp, .id)
      )
    | .dateRange = {
        after: ($existing.dateRange.after // $incremental.dateRange.after),
        before: ($incremental.dateRange.before // $existing.dateRange.before)
      }
    | .exportedAt = ($incremental.exportedAt // $existing.exportedAt)
    | if ($existing | has("messageCount")) or ($incremental | has("messageCount"))
      then .messageCount = (.messages | length)
      else .
      end
  ' "$existing_path" "$incremental_path" >"$merged_path"
}

load_guild_cache() {
  local output

  if [[ ! -f "$CACHE_ROOT/guilds.tsv" ]]; then
    if ! output=$("$CLI_BIN" guilds 2>&1); then
      die "Guild discovery failed. If you are using a bot token, configure explicit guild_ids/channel_ids for each non-DM target or switch to a user token. CLI output: $output"
    fi

    printf '%s\n' "$output" | parse_two_column_listing >"$CACHE_ROOT/guilds.tsv"
  fi

  cat "$CACHE_ROOT/guilds.tsv"
}

load_dm_channel_cache() {
  local output

  if [[ ! -f "$CACHE_ROOT/dms.txt" ]]; then
    if ! output=$("$CLI_BIN" dm 2>&1); then
      die "DM discovery failed. Bot tokens cannot read direct messages; disable the DM target or switch to a user token. CLI output: $output"
    fi

    printf '%s\n' "$output" | parse_channel_listing >"$CACHE_ROOT/dms.txt"
  fi

  cat "$CACHE_ROOT/dms.txt"
}

load_guild_channel_cache() {
  local guild_id=$1
  local include_voice=$2
  local include_threads=$3
  local cache_file="$CACHE_ROOT/channels_${guild_id}_${include_voice}_${include_threads}.txt"

  if [[ ! -f "$cache_file" ]]; then
    local output
    if ! output=$("$CLI_BIN" channels \
      --guild "$guild_id" \
      --include-vc "$include_voice" \
      --include-threads "$include_threads" 2>&1); then
      die "Channel discovery failed for guild $guild_id. CLI output: $output"
    fi

    printf '%s\n' "$output" | parse_channel_listing >"$cache_file"
  fi

  cat "$cache_file"
}

resolve_guild_ids_from_patterns() {
  local patterns=("$@")
  local guild_id guild_name normalized_guild normalized_pattern pattern

  (( ${#patterns[@]} > 0 )) || return 0

  while IFS=$'\t' read -r guild_id guild_name; do
    normalized_guild=$(normalize_name "$guild_name")

    for pattern in "${patterns[@]}"; do
      normalized_pattern=$(normalize_name "$pattern")
      [[ -n "$normalized_pattern" ]] || continue

      if [[ "$normalized_guild" == "$normalized_pattern" || "$normalized_guild" == *"$normalized_pattern"* || "$normalized_pattern" == *"$normalized_guild"* ]]; then
        printf '%s\n' "$guild_id"
        break
      fi
    done
  done < <(load_guild_cache)
}

resolve_configured_guilds() {
  local target_json=$1
  local -a configured_guild_ids name_patterns resolved_guild_ids

  mapfile -t configured_guild_ids < <(jq -r '.guild_ids[]? | tostring' <<<"$target_json")
  mapfile -t name_patterns < <(jq -r '.guild_name_patterns[]?' <<<"$target_json")

  if (( ${#configured_guild_ids[@]} > 0 )); then
    printf '%s\n' "${configured_guild_ids[@]}" | sort -u
    return 0
  fi

  mapfile -t resolved_guild_ids < <(resolve_guild_ids_from_patterns "${name_patterns[@]}" | sort -u)
  if (( ${#resolved_guild_ids[@]} == 0 )); then
    return 0
  fi

  if (( ${#resolved_guild_ids[@]} > 1 )); then
    die "Target '$(jq -r '.name' <<<"$target_json")' matched multiple guilds (${resolved_guild_ids[*]}). Configure explicit guild_ids to make it safe."
  fi

  printf '%s\n' "${resolved_guild_ids[@]}"
}

resolve_target_channels() {
  local target_json=$1
  local defaults_json=$2
  local kind include_voice include_threads
  local target_name output_dir
  local -a configured_channel_ids configured_guild_ids seeded_channel_ids allowed_channels allowed_guilds selected_guilds

  target_name=$(jq -r '.name' <<<"$target_json")
  output_dir=$(jq -r '.output_dir' <<<"$target_json")
  kind=$(jq -r '.kind // "guild"' <<<"$target_json")
  include_voice=$(jq -r --argjson defaults "$defaults_json" '(.include_voice_channels // $defaults.include_voice_channels // false) | tostring' <<<"$target_json")
  include_threads=$(jq -r --argjson defaults "$defaults_json" '.include_threads // $defaults.include_threads // "all"' <<<"$target_json")

  mapfile -t configured_channel_ids < <(jq -r '.channel_ids[]? | tostring' <<<"$target_json")
  mapfile -t seeded_channel_ids < <(load_archive_seed_channel_ids "$output_dir" | sort -u)

  if [[ "$kind" == "dms" ]]; then
    (( ${#OVERRIDE_GUILDS[@]} == 0 )) || die "DM targets do not support --guild overrides."
    mapfile -t allowed_channels < <(load_dm_channel_cache | sort -u)

    if (( ${#OVERRIDE_CHANNELS[@]} > 0 )); then
      assert_subset "Channel override" OVERRIDE_CHANNELS allowed_channels
      printf '%s\n' "${OVERRIDE_CHANNELS[@]}" | sort -u
    else
      printf '%s\n' "${allowed_channels[@]}"
    fi
    return 0
  fi

  if (( ${#configured_channel_ids[@]} > 0 )); then
    (( ${#OVERRIDE_GUILDS[@]} == 0 )) || die "Channel-scoped targets do not support --guild overrides."
    mapfile -t allowed_channels < <(printf '%s\n' "${configured_channel_ids[@]}" | sort -u)

    if (( ${#OVERRIDE_CHANNELS[@]} > 0 )); then
      assert_subset "Channel override" OVERRIDE_CHANNELS allowed_channels
      printf '%s\n' "${OVERRIDE_CHANNELS[@]}" | sort -u
    else
      printf '%s\n' "${allowed_channels[@]}"
    fi
    return 0
  fi

  if (( ${#seeded_channel_ids[@]} > 0 )); then
    (( ${#OVERRIDE_GUILDS[@]} == 0 )) || die "Archive-seeded target '$target_name' does not support --guild overrides."
    mapfile -t allowed_channels < <(printf '%s\n' "${seeded_channel_ids[@]}" | sort -u)

    if (( ${#OVERRIDE_CHANNELS[@]} > 0 )); then
      assert_subset "Channel override" OVERRIDE_CHANNELS allowed_channels
      printf '%s\n' "${OVERRIDE_CHANNELS[@]}" | sort -u
    else
      printf '%s\n' "${allowed_channels[@]}"
    fi
    return 0
  fi

  mapfile -t configured_guild_ids < <(resolve_configured_guilds "$target_json")
  if (( ${#configured_guild_ids[@]} == 0 )); then
    return 0
  fi

  if (( ${#OVERRIDE_GUILDS[@]} > 0 )); then
    assert_subset "Guild override" OVERRIDE_GUILDS configured_guild_ids
    selected_guilds=("${OVERRIDE_GUILDS[@]}")
  else
    selected_guilds=("${configured_guild_ids[@]}")
  fi

  local guild_id
  mapfile -t allowed_channels < <(
    for guild_id in "${selected_guilds[@]}"; do
      load_guild_channel_cache "$guild_id" "$include_voice" "$include_threads"
    done | sort -u
  )

  if (( ${#OVERRIDE_CHANNELS[@]} > 0 )); then
    assert_subset "Channel override" OVERRIDE_CHANNELS allowed_channels
    printf '%s\n' "${OVERRIDE_CHANNELS[@]}" | sort -u
  else
    printf '%s\n' "${allowed_channels[@]}"
  fi
}

preflight_probe_channel() {
  local probe_channel_id=$1
  local output_dir=$2
  local probe_dir probe_output probe_log
  local -a probe_command after_id probe_destination
  local probe_status=0

  probe_dir=$(mktemp -d "${TMPDIR:-/tmp}/dce-preflight.${probe_channel_id}.XXXXXX")
  probe_output="$probe_dir/probe.json"
  probe_log=$(mktemp "${TMPDIR:-/tmp}/dce-preflight-log.${probe_channel_id}.XXXXXX")

  probe_destination=$(resolve_destination_path "$output_dir" "$probe_channel_id")
  after_id=""
  if [[ -n "$probe_destination" && -f "$probe_destination" ]]; then
    after_id=$(last_message_id "$probe_destination")
  fi

  probe_command=(
    "$CLI_BIN" export
    --channel "$probe_channel_id"
    --format Json
    --output "$probe_output"
    --partition 1
  )
  if [[ -n "$after_id" ]]; then
    probe_command+=(--after "$after_id")
  fi

  set +e
  "${probe_command[@]}" >"$probe_log" 2>&1
  probe_status=$?
  set -e

  if (( probe_status == 0 )); then
    rm -f "$probe_log"
    rm -rf "$probe_dir"
    return 0
  fi

  if is_skippable_channel_export_failure "$probe_log"; then
    log "Preflight probe skipped channel $probe_channel_id (forbidden or inaccessible)."
    cat "$probe_log" >&2
    rm -f "$probe_log"
    rm -rf "$probe_dir"
    return 2
  fi

  cat "$probe_log" >&2
  rm -f "$probe_log"
  rm -rf "$probe_dir"
  return 1
}

preflight_target() {
  local target_json=$1
  local defaults_json=$2
  local target_name output_dir
  local probe_channel_id
  local -a channel_ids seeded_channel_ids
  local probe_status=0
  local skipped_channels=0
  local probed_channels=0

  target_name=$(jq -r '.name' <<<"$target_json")
  output_dir=$(jq -r '.output_dir' <<<"$target_json")
  bootstrap_channel_map_from_archives "$output_dir"

  mapfile -t channel_ids < <(resolve_target_channels "$target_json" "$defaults_json")
  if (( ${#channel_ids[@]} == 0 )); then
    die "Target '$target_name' resolved no channels during preflight."
  fi

  for probe_channel_id in "${channel_ids[@]}"; do
    probed_channels=$((probed_channels + 1))
    preflight_probe_channel "$probe_channel_id" "$output_dir" || probe_status=$?
    case "$probe_status" in
      0)
        log "Preflight ok for target '$target_name': token verified (${#channel_ids[@]} channel(s) resolved, probe succeeded on $probe_channel_id) for $output_dir."
        return 0
        ;;
      2)
        skipped_channels=$((skipped_channels + 1))
        probe_status=0
        ;;
      *)
        die "Target '$target_name' failed authenticated preflight on channel '$probe_channel_id'."
        ;;
    esac
  done

  mapfile -t seeded_channel_ids < <(load_archive_seed_channel_ids "$output_dir" | sort -u)
  if (( skipped_channels == probed_channels && ${#seeded_channel_ids[@]} > 0 )); then
    log "Preflight ok for target '$target_name' with warning: all ${#channel_ids[@]} resolved channel(s) are inaccessible, but ${#seeded_channel_ids[@]} seeded archive(s) exist under $output_dir."
    return 0
  fi

  die "Target '$target_name' failed preflight: every resolved channel is inaccessible and no seeded archives exist under $output_dir."
}

scrape_target() {
  local target_json=$1
  local defaults_json=$2
  local target_name output_dir destination_path after_id temp_dir temp_export temp_merged
  local latest_batch_count
  local -a channel_ids
  local export_status=0

  target_name=$(jq -r '.name' <<<"$target_json")
  output_dir=$(jq -r '.output_dir' <<<"$target_json")
  mkdir -p "$output_dir"
  bootstrap_channel_map_from_archives "$output_dir"

  mapfile -t channel_ids < <(resolve_target_channels "$target_json" "$defaults_json")
  if (( ${#channel_ids[@]} == 0 )); then
    die "Target '$target_name' resolved no channels."
  fi

  log "Target '$target_name': processing ${#channel_ids[@]} channel(s) into $output_dir."

  local channel_id
  local skipped_channels=0
  for channel_id in "${channel_ids[@]}"; do
    destination_path=$(resolve_destination_path "$output_dir" "$channel_id")
    if [[ -n "$destination_path" ]]; then
      mkdir -p "$(dirname "$destination_path")"
    fi

    if [[ -n "$destination_path" && -f "$destination_path" ]]; then
      jq empty "$destination_path" >/dev/null 2>&1 || die "Existing export is not valid JSON: $destination_path"
      assert_export_channel_identity "$destination_path" "$channel_id"
    fi

    after_id=$(last_message_id "$destination_path")
    mkdir -p "$output_dir/.dce-temp"
    temp_dir=$(mktemp -d "$output_dir/.dce-temp/export.${channel_id}.XXXXXX")
    temp_export="$temp_dir/export.json"
    temp_merged="$temp_dir/merged.json"

    log "Exporting channel $channel_id for target '$target_name'${after_id:+ after message $after_id}."

    export_status=0
    export_channel_incremental "$channel_id" "$temp_export" "$after_id" || export_status=$?
    case "$export_status" in
      0) ;;
      2)
        rm -rf "$temp_dir"
        skipped_channels=$((skipped_channels + 1))
        continue
        ;;
      *)
        rm -rf "$temp_dir"
        die "Channel $channel_id failed for target '$target_name'."
        ;;
    esac

    jq empty "$temp_export" >/dev/null 2>&1 || die "Incremental export is not valid JSON: $temp_export"
    assert_export_channel_identity "$temp_export" "$channel_id"

    if [[ -z "$destination_path" ]]; then
      destination_path=$(resolve_destination_path "$output_dir" "$channel_id" "$temp_export")
      mkdir -p "$(dirname "$destination_path")"
    fi

    latest_batch_count=$(message_count "$temp_export")
    if [[ ! -f "$destination_path" ]]; then
      mv "$temp_export" "$destination_path"
      rm -rf "$temp_dir"
      continue
    fi

    if (( latest_batch_count == 0 )); then
      rm -rf "$temp_dir"
      continue
    fi

    merge_exports "$destination_path" "$temp_export" "$temp_merged"
    [[ -s "$temp_merged" ]] || die "Merged export is empty for channel $channel_id."
    jq empty "$temp_merged" >/dev/null 2>&1 || die "Merged export is not valid JSON: $temp_merged"
    assert_export_channel_identity "$temp_merged" "$channel_id"
    commit_merged_export "$destination_path" "$temp_merged"
    rm -rf "$temp_dir"
  done

  if (( skipped_channels > 0 )); then
    log "Target '$target_name': skipped $skipped_channels inaccessible channel(s)."
  fi

  log "Target '$target_name': scrape completed successfully."
}

list_targets() {
  local config_path=$1

  validate_config_contract "$config_path"
  jq -r '.targets[] | [.name, (.kind // "guild"), .output_dir] | @tsv' "$config_path"
}

load_selected_targets() {
  local config_path=$1
  shift
  local -a requested_targets=("$@")
  local target_names_json

  if (( ${#requested_targets[@]} > 0 )); then
    target_names_json=$(json_array_from_args "${requested_targets[@]}")
    jq -c --argjson selected_target_names "$target_names_json" \
      '.targets[] | select(.name as $name | $selected_target_names | index($name))' \
      "$config_path"
  else
    jq -c '.targets[] | select(.enabled != false)' "$config_path"
  fi
}

parse_target_options() {
  local mode=$1
  shift
  local -n config_path_ref=$1
  local -n requested_targets_ref=$2
  shift 2

  while (($#)); do
    case "$1" in
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        config_path_ref=$2
        shift 2
        ;;
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        requested_targets_ref+=("$2")
        shift 2
        ;;
      --guild)
        [[ $# -ge 2 ]] || die "Missing value for --guild."
        OVERRIDE_GUILDS+=("$2")
        shift 2
        ;;
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        OVERRIDE_CHANNELS+=("$2")
        shift 2
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      *)
        die "Unknown $mode option: $1"
        ;;
    esac
  done
}

run_target_mode() {
  local mode=$1
  local config_path requested_targets_json defaults_json
  local -a requested_targets=() selected_targets=()
  shift

  config_path=$(default_config_path)
  parse_target_options "$mode" config_path requested_targets "$@"

  require_command jq
  validate_config_contract "$config_path"
  [[ -n "${DISCORD_TOKEN:-}" ]] || die "DISCORD_TOKEN is not set."

  defaults_json=$(jq -c '.defaults // {}' "$config_path")
  mapfile -t selected_targets < <(load_selected_targets "$config_path" "${requested_targets[@]}")

  if (( ${#requested_targets[@]} > 0 && ${#selected_targets[@]} != ${#requested_targets[@]} )); then
    die "One or more requested --target names are not present in $config_path."
  fi

  if (( ${#selected_targets[@]} == 0 )); then
    if (( ${#requested_targets[@]} > 0 )); then
      die "No targets matched the requested selection."
    fi
    die "No enabled targets are available in $config_path."
  fi

  if (( (${#OVERRIDE_GUILDS[@]} > 0 || ${#OVERRIDE_CHANNELS[@]} > 0) && ${#selected_targets[@]} != 1 )); then
    die "When using --guild or --channel overrides, select exactly one --target."
  fi

  CACHE_ROOT=$(mktemp -d "${TMPDIR:-/tmp}/dce-scrape.XXXXXX")
  trap 'rm -rf "$CACHE_ROOT"' EXIT

  local target_json
  for target_json in "${selected_targets[@]}"; do
    if [[ "$mode" == "preflight" ]]; then
      preflight_target "$target_json" "$defaults_json"
    else
      scrape_target "$target_json" "$defaults_json"
    fi
  done
}

main() {
  local subcommand=${1:-help}
  local config_path
  shift || true

  case "$subcommand" in
    help|-h|--help)
      usage
      ;;
    list-targets)
      config_path=$(default_config_path)
      while (($#)); do
        case "$1" in
          --config)
            [[ $# -ge 2 ]] || die "Missing value for --config."
            config_path=$2
            shift 2
            ;;
          *)
            die "Unknown list-targets option: $1"
            ;;
        esac
      done
      list_targets "$config_path"
      ;;
    preflight)
      run_target_mode preflight "$@"
      ;;
    scrape)
      run_target_mode scrape "$@"
      ;;
    *)
      exec "$CLI_BIN" "$subcommand" "$@"
      ;;
  esac
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
  main "$@"
fi
