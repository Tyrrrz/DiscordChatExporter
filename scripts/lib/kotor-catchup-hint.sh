#!/usr/bin/env bash

# Shared post-check hint when KotOR_discord_msgs is enabled in scrape config.

kotor_catchup_enabled() {
  local config_path=$1
  command -v jq >/dev/null 2>&1 || return 1
  [[ -f "$config_path" ]] || return 1
  jq -e '.targets[] | select(.name == "KotOR_discord_msgs" and (.enabled // true) == true)' \
    "$config_path" >/dev/null 2>&1
}

print_kotor_catchup_hint() {
  local config_path=$1
  kotor_catchup_enabled "$config_path" || return 0
  printf '\nKotOR yes_general catch-up (channel 221726893064454144):\n'
  printf '  ./scripts/run-kotor-yes-general-catchup.sh\n'
  printf '  ./scripts/print-scrape-summary.sh logs/kotor-yes-general.summary.json\n'
}
