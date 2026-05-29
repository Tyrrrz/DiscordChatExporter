#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
CONFIG_PATH="${DCE_CONFIG_FILE:-$REPO_ROOT/config/scrape-targets.json}"
LOG_DIR="${DCE_LOG_DIR:-$REPO_ROOT/logs}"
SYNC_GUI="$REPO_ROOT/scripts/sync-token-from-gui.sh"
VERIFY_READY="$REPO_ROOT/scripts/verify-operator-ready.sh"
DOCUMENTS_SCRAPE="$REPO_ROOT/scripts/run-documents-scrape.sh"
AUDIT_JSON="$REPO_ROOT/scripts/audit-archive-json.sh"

DRY_RUN=0
SKIP_SCRAPE=0
SYNC_GUI_FLAG=0
TARGET=""
LOG_FILE=""

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [options]

End-to-end operator validation with timestamped log:
  optional GUI token sync → verify-operator-ready → documents scrape → JSON audit

Options:
  --dry-run       Readiness + archives only (no Discord scrape)
  --skip-scrape   Readiness only (no scrape, no audit loop)
  --sync-gui      Run sync-token-from-gui.sh --force before checks
  --target NAME   Limit scrape/audit to one configured target
  --config PATH   Targets JSON (default: config/scrape-targets.json)
  --log-file PATH Append output to this file (default: logs/operator-validation-UTC.log)
  --help          Show this help text
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

log_step() {
  printf '[%s] %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$*"
}

run_step() {
  local label=$1
  shift
  log_step "BEGIN: $label"
  "$@"
  local status=$?
  log_step "END: $label (exit $status)"
  return "$status"
}

audit_targets() {
  if [[ -n "$TARGET" ]]; then
    run_step "audit-archive-json ($TARGET)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$TARGET"
    return
  fi
  local name
  while IFS= read -r name; do
    [[ -n "$name" ]] || continue
    run_step "audit-archive-json ($name)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$name" || return 1
  done < <(jq -r '.targets[] | select(.enabled != false) | .name' "$CONFIG_PATH")
}

main() {
  while (($#)); do
    case "$1" in
      --dry-run)
        DRY_RUN=1
        shift
        ;;
      --skip-scrape)
        SKIP_SCRAPE=1
        shift
        ;;
      --sync-gui)
        SYNC_GUI_FLAG=1
        shift
        ;;
      --target)
        [[ $# -ge 2 ]] || die "Missing value for --target."
        TARGET=$2
        shift 2
        ;;
      --config)
        [[ $# -ge 2 ]] || die "Missing value for --config."
        CONFIG_PATH=$2
        shift 2
        ;;
      --log-file)
        [[ $# -ge 2 ]] || die "Missing value for --log-file."
        LOG_FILE=$2
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

  mkdir -p "$LOG_DIR"
  if [[ -z "$LOG_FILE" ]]; then
    LOG_FILE="$LOG_DIR/operator-validation-$(date -u +%Y%m%dT%H%M%SZ).log"
  fi

  local failures=0

  set -o pipefail
  {
    log_step "Operator validation started (config=$CONFIG_PATH)"
    if (( SYNC_GUI_FLAG )); then
      run_step "sync-token-from-gui" "$SYNC_GUI" --force || failures=$((failures + 1))
    fi

    run_step "verify-operator-ready" "$VERIFY_READY" --config "$CONFIG_PATH" || failures=$((failures + 1))

    if (( SKIP_SCRAPE )); then
      log_step "Skip scrape requested."
    else
      local -a scrape_args=(--config "$CONFIG_PATH")
      [[ -n "$TARGET" ]] && scrape_args+=(--target "$TARGET")
      if (( DRY_RUN )); then
        scrape_args+=(--dry-run)
      fi
      run_step "run-documents-scrape" "$DOCUMENTS_SCRAPE" "${scrape_args[@]}" || failures=$((failures + 1))
      if (( DRY_RUN == 0 && failures == 0 )); then
        audit_targets || failures=$((failures + 1))
      fi
    fi

    if (( failures > 0 )); then
      log_step "Operator validation failed ($failures step(s))."
      exit 1
    fi
    log_step "Operator validation finished successfully."
  } 2>&1 | tee -a "$LOG_FILE"
  local pipeline_status=${PIPESTATUS[0]}

  printf 'Log: %s\n' "$LOG_FILE"
  exit "$pipeline_status"
}

main "$@"
