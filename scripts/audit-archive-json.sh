#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
TARGET=""
FAILURES=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--config PATH] [--target NAME]

Validate JSON syntax for every channel export under configured targets.
Exits 1 when any invalid file is found.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

audit_dir() {
  local output_dir=$1
  local file_path

  [[ -d "$output_dir" ]] || return 0

  while IFS= read -r -d '' file_path; do
    if jq empty "$file_path" >/dev/null 2>&1; then
      continue
    fi
    printf 'INVALID\t%s\n' "$file_path"
    FAILURES=$((FAILURES + 1))
  done < <(find "$output_dir" -type f -name '*.json' ! -path '*/.dce-meta/*' ! -path '*/.dce-temp/*' -print0 2>/dev/null)
}

main() {
  while (($#)); do
    case "$1" in
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        TARGET=$2
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

  command -v jq >/dev/null 2>&1 || die "jq is required."
  [[ -f "$CONFIG_PATH" ]] || die "Missing config: $CONFIG_PATH"

  if [[ -n "$TARGET" ]]; then
    local output_dir
    output_dir=$(jq -r --arg name "$TARGET" '.targets[] | select(.name == $name) | .output_dir' "$CONFIG_PATH")
    [[ -n "$output_dir" && "$output_dir" != null ]] || die "Unknown target: $TARGET"
    audit_dir "$output_dir"
  else
    while IFS= read -r output_dir; do
      [[ -n "$output_dir" ]] || continue
      audit_dir "$output_dir"
    done < <(jq -r '.targets[] | select(.enabled != false) | .output_dir' "$CONFIG_PATH")
  fi

  if (( FAILURES > 0 )); then
    printf '\n%d invalid JSON archive file(s). Run scripts/salvage-truncated-export.sh on each path.\n' "$FAILURES" >&2
    exit 1
  fi

  printf 'All checked archive JSON files are valid.\n'
}

main "$@"
