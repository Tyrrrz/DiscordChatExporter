#!/usr/bin/env bash

set -Eeuo pipefail

OOM_ONLY=0
RAW_JSON=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--json] [--oom-only] FILE|-

Pretty-print a scrape run summary JSON file (version 1) from operator validation,
operator proof, or DCE_RUN_SUMMARY_FILE exports.

  --json      Print raw JSON unchanged
  --oom-only  List only SKIPPED_OOM channels
  FILE        Path to *.summary.json (use - for stdin)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

validate_summary() {
  local file=$1
  jq -e '.version == 1 and (.totals | type) == "object" and (.channels | type) == "array"' "$file" >/dev/null \
    || die "Invalid or unsupported summary JSON: $file"
}

print_summary() {
  local file=$1
  local finished_at channel_count

  validate_summary "$file"

  if (( RAW_JSON )); then
    cat "$file"
    return 0
  fi

  finished_at=$(jq -r '.finished_at // "unknown"' "$file")
  printf 'Scrape summary (finished %s)\n' "$finished_at"
  jq -r '
    .totals
    | "Totals: \(.created) created, \(.merged) merged, \(.unchanged) unchanged, \(.skipped) skipped (\(.skipped_oom) OOM); +\(.messages_appended) messages appended"
  ' "$file"

  channel_count=$(jq -r '.channels | length' "$file")
  if (( channel_count == 0 )); then
    printf '\nNo channel activity recorded.\n'
    return 0
  fi

  if (( OOM_ONLY )); then
    local oom_count
    oom_count=$(jq -r '[.channels[] | select(.action == "SKIPPED_OOM")] | length' "$file")
    if (( oom_count == 0 )); then
      printf '\nNo OOM/aborted channel skips recorded.\n'
      return 0
    fi
    printf '\nOOM/aborted skips:\n'
    jq -r '
      .channels[]
      | select(.action == "SKIPPED_OOM")
      | "  channel \(.channel_id)  \(.guild_label)  target=\(.target)"
    ' "$file"
    return 0
  fi

  printf '\n%-12s %-20s %-20s %8s %s\n' ACTION CHANNEL LABEL DELTA FILE
  jq -r '
    .channels[]
    | [
        (if .action == "SKIPPED_OOM" then "SKIPPED(OOM)" else .action end),
        .channel_id,
        .guild_label,
        (if .delta >= 0 then "+" + (.delta | tostring) else (.delta | tostring) end),
        (.file_path // "")
      ]
    | @tsv
  ' "$file" | while IFS=$'\t' read -r action channel_id guild_label delta file_path; do
    printf '%-12s %-20s %-20s %8s %s\n' "$action" "$channel_id" "$guild_label" "$delta" "$file_path"
  done
}

main() {
  local summary_file=""

  while (($#)); do
    case "$1" in
      --json)
        RAW_JSON=1
        shift
        ;;
      --oom-only)
        OOM_ONLY=1
        shift
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      -)
        [[ -z "$summary_file" ]] || die "Unexpected extra argument: $1"
        summary_file=-
        shift
        ;;
      --)
        shift
        break
        ;;
      -*)
        die "Unknown option: $1"
        ;;
      *)
        [[ -z "$summary_file" ]] || die "Unexpected extra argument: $1"
        summary_file=$1
        shift
        ;;
    esac
  done

  [[ -n "$summary_file" ]] || {
    usage >&2
    die "Missing summary file path."
  }

  require_command jq

  local input_file
  if [[ "$summary_file" == "-" ]]; then
    input_file=$(mktemp "${TMPDIR:-/tmp}/dce-summary-in.XXXXXX.json")
    trap "rm -f '$input_file'" EXIT
    cat >"$input_file"
    print_summary "$input_file"
  else
    [[ -f "$summary_file" && -r "$summary_file" ]] || die "Summary file not found: $summary_file"
    print_summary "$summary_file"
  fi
}

main "$@"
