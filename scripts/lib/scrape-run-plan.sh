#!/usr/bin/env bash

# Host-side helpers: list selected/enabled targets before container runs.

print_scrape_config_plan() {
  local config_path=$1
  local action_label=$2
  shift 2
  local -a requested_targets=("$@")
  local target_names_json line name output_dir enabled_count

  [[ -f "$config_path" ]] || return 0

  printf '%s\n' "=== $action_label run plan ==="
  printf 'Config: %s\n' "$config_path"

  if (( ${#requested_targets[@]} > 0 )); then
    printf 'Targets (%s selected):\n' "${#requested_targets[@]}"
    for name in "${requested_targets[@]}"; do
      output_dir=$(jq -r --arg name "$name" '.targets[] | select(.name == $name) | .output_dir' "$config_path")
      printf '  - %s → %s\n' "$name" "$output_dir"
    done
    return 0
  fi

  enabled_count=$(jq -r '[.targets[] | select(.enabled != false)] | length' "$config_path")
  printf 'Targets (%s enabled, all will run):\n' "$enabled_count"
  while IFS=$'\t' read -r name output_dir; do
    [[ -n "$name" ]] || continue
    printf '  - %s → %s\n' "$name" "$output_dir"
  done < <(jq -r '.targets[] | select(.enabled != false) | [.name, .output_dir] | @tsv' "$config_path")
}

enabled_target_names() {
  local config_path=$1
  jq -r '.targets[] | select(.enabled != false) | .name' "$config_path"
}
