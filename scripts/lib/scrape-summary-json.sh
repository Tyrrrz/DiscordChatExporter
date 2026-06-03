#!/usr/bin/env bash

# Recover machine-readable scrape summaries from teed operator logs.

extract_json_summary_from_log() {
  local source_log=$1
  local dest_file=$2
  local line json_payload

  [[ -n "$source_log" && -n "$dest_file" ]] || return 1
  [[ -f "$source_log" && -r "$source_log" ]] || return 1
  command -v jq >/dev/null 2>&1 || return 1

  line=$(grep 'DCE_JSON_SUMMARY:' "$source_log" | tail -1) || return 1
  [[ -n "$line" ]] || return 1

  json_payload=${line#*DCE_JSON_SUMMARY: }
  [[ -n "$json_payload" ]] || return 1

  if ! jq -e . >/dev/null 2>&1 <<<"$json_payload"; then
    return 1
  fi

  mkdir -p "$(dirname "$dest_file")"
  jq . <<<"$json_payload" >"$dest_file"
}

recover_json_summary_if_missing() {
  local run_log=$1
  local dest_file=$2

  [[ -n "$run_log" && -n "$dest_file" ]] || return 1
  [[ -s "$dest_file" ]] && return 1
  extract_json_summary_from_log "$run_log" "$dest_file"
}
