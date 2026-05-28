#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
READ_GUI_TOKEN="$REPO_ROOT/scripts/read-dce-gui-token.sh"

discover_settings_dat() {
  local candidate

  if [[ -n "${DISCORDCHATEXPORTER_SETTINGS_PATH:-}" ]]; then
    if [[ -f "${DISCORDCHATEXPORTER_SETTINGS_PATH}" ]]; then
      printf '%s\n' "${DISCORDCHATEXPORTER_SETTINGS_PATH}"
      return 0
    fi
    if [[ -f "${DISCORDCHATEXPORTER_SETTINGS_PATH}/Settings.dat" ]]; then
      printf '%s\n' "${DISCORDCHATEXPORTER_SETTINGS_PATH}/Settings.dat"
      return 0
    fi
  fi

  for candidate in \
    "$REPO_ROOT/../DiscordChatExporter.linux-x64/Settings.dat" \
    "$HOME/Downloads/DiscordChatExporter.linux-x64/Settings.dat" \
    "$REPO_ROOT/Settings.dat"; do
    if [[ -f "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  return 1
}

try_gui_settings_token() {
  local settings_path token

  settings_path=$(discover_settings_dat) || return 1
  [[ -x "$READ_GUI_TOKEN" ]] || return 1
  token=$("$READ_GUI_TOKEN" "$settings_path" 2>/dev/null) || return 1
  [[ -n "$token" ]] || return 1
  printf '%s' "$token"
}

try_discord_client_token() {
  python3 - <<'PY' 2>/dev/null || return 1
import re
from pathlib import Path

root = Path.home() / ".config/discord/Local Storage/leveldb"
if not root.is_dir():
    raise SystemExit(1)

pattern = re.compile(rb"[\w-]{24}\.[\w-]{6}\.[\w-]{27,}")
seen = []
for entry in root.iterdir():
    if not entry.is_file():
        continue
    try:
        data = entry.read_bytes()
    except OSError:
        continue
    for match in pattern.finditer(data):
        token = match.group().decode("ascii", "ignore")
        if len(token) > 50 and token not in seen:
            seen.append(token)

if not seen:
    raise SystemExit(1)

seen.sort(key=len, reverse=True)
print(seen[0], end="")
PY
}

main() {
  if [[ -n "${DISCORD_TOKEN:-}" ]]; then
    printf '%s' "$DISCORD_TOKEN"
    exit 0
  fi

  if [[ -n "${DISCORD_TOKEN_FILE:-}" && -f "${DISCORD_TOKEN_FILE}" ]]; then
    head -n 1 "$DISCORD_TOKEN_FILE" | tr -d '\r'
    exit 0
  fi

  for candidate in \
    "$REPO_ROOT/.discord-token" \
    "$HOME/.config/discord-scrape/token" \
    "$HOME/.config/discord-token"; do
    if [[ -f "$candidate" ]]; then
      head -n 1 "$candidate" | tr -d '\r'
      exit 0
    fi
  done

  if token=$(try_gui_settings_token); then
    printf '%s' "$token"
    exit 0
  fi

  if token=$(try_discord_client_token); then
    printf '%s' "$token"
    exit 0
  fi

  exit 1
}

main "$@"
