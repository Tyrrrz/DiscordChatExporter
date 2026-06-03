# Shared scrape lock path and inspection helpers.
# Sourced by run-discord-scrape-host.sh, scrape-lock-status.sh, and operator wrappers.

resolve_scrape_lock_file() {
  local config_path=$1 repo_root=$2

  if [[ -n "${DCE_SCRAPE_LOCK_FILE:-}" ]]; then
    printf '%s\n' "$DCE_SCRAPE_LOCK_FILE"
    return 0
  fi

  local archive_root=""
  if [[ -f "$config_path" ]]; then
    archive_root=$(jq -r '.archive_root // empty' "$config_path" 2>/dev/null) || true
  fi
  if [[ -n "$archive_root" && "$archive_root" != null ]]; then
    printf '%s/.dce-scrape.lock\n' "$archive_root"
  else
    printf '%s/.dce-scrape.lock\n' "$repo_root"
  fi
}

scrape_lock_meta_path() {
  printf '%s.meta\n' "$1"
}

read_scrape_lock_meta_field() {
  local meta_file=$1 field=$2
  grep -E "^${field}=" "$meta_file" 2>/dev/null | head -1 | cut -d= -f2- || true
}

scrape_lock_is_held() {
  local lock_file=$1

  command -v flock >/dev/null 2>&1 || return 1
  exec {lock_probe_fd}>>"$lock_file"
  if flock -n "$lock_probe_fd"; then
    flock -u "$lock_probe_fd" 2>/dev/null || true
    exec {lock_probe_fd}>&-
    return 1
  fi
  exec {lock_probe_fd}>&-
  return 0
}

scrape_lock_format_holder_summary() {
  local meta_file=$1
  local pid="" started="" cmd="" holder_state=""

  [[ -f "$meta_file" ]] || return 0
  pid=$(read_scrape_lock_meta_field "$meta_file" pid)
  started=$(read_scrape_lock_meta_field "$meta_file" started)
  cmd=$(read_scrape_lock_meta_field "$meta_file" cmd)
  [[ -n "$pid" ]] || return 0

  if kill -0 "$pid" 2>/dev/null; then
    holder_state="running"
  else
    holder_state="not running"
  fi
  printf 'Holder pid %s (%s, started %s): %s' "$pid" "$holder_state" "${started:-unknown}" "${cmd:-unknown}"
}

scrape_lock_format_holder_lines() {
  local meta_file=$1
  local pid="" started="" cmd="" holder_state=""

  [[ -f "$meta_file" ]] || return 0
  pid=$(read_scrape_lock_meta_field "$meta_file" pid)
  started=$(read_scrape_lock_meta_field "$meta_file" started)
  cmd=$(read_scrape_lock_meta_field "$meta_file" cmd)
  [[ -n "$pid" ]] || return 0

  if kill -0 "$pid" 2>/dev/null; then
    holder_state="running"
  else
    holder_state="not running"
  fi
  printf 'holder: pid %s (%s, started %s)\n' "$pid" "$holder_state" "${started:-unknown}"
  [[ -n "$cmd" ]] && printf 'cmd: %s\n' "$cmd"
}

scrape_lock_try_reclaim_meta() {
  local meta_file=$1
  local pid

  [[ -f "$meta_file" ]] || return 1
  pid=$(read_scrape_lock_meta_field "$meta_file" pid)
  [[ -n "$pid" ]] || return 1
  if kill -0 "$pid" 2>/dev/null; then
    return 1
  fi
  rm -f "$meta_file"
  return 0
}

scrape_lock_reclaim_stale_files() {
  local lock_file=$1 meta_file=$2

  if scrape_lock_is_held "$lock_file"; then
    return 2
  fi

  if [[ -f "$meta_file" ]]; then
    local pid
    pid=$(read_scrape_lock_meta_field "$meta_file" pid)
    if [[ -n "$pid" ]] && kill -0 "$pid" 2>/dev/null; then
      return 3
    fi
    rm -f "$meta_file"
    printf 'removed stale lock meta: %s\n' "$meta_file"
  fi

  if [[ -e "$lock_file" ]] && ! scrape_lock_is_held "$lock_file"; then
    rm -f "$lock_file"
    printf 'removed unheld lock file: %s\n' "$lock_file"
  fi
  return 0
}

ensure_scrape_lock_available() {
  local config_path=$1 status_script=$2

  if [[ "${DCE_SKIP_SCRAPE_LOCK:-0}" == "1" ]]; then
    return 0
  fi
  [[ -x "$status_script" ]] || return 0
  if ! "$status_script" --config "$config_path"; then
    return 1
  fi
  return 0
}
