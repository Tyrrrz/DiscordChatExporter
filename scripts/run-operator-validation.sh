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
LOCK_STATUS="$REPO_ROOT/scripts/scrape-lock-status.sh"
# shellcheck source=lib/scrape-lock.sh
source "$SCRIPT_DIR/lib/scrape-lock.sh"

DRY_RUN=0
SKIP_SCRAPE=0
SALVAGE_ONLY=0
SALVAGE_BEFORE=0
SYNC_GUI_FLAG=0
PER_TARGET=0
CONTINUE_ON_ERROR=0
TARGET=""
CHANNEL_ARGS=()
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
  --salvage-only  Merge stale .dce-temp exports only, then audit (no Discord scrape)
  --salvage-before-scrape  Run salvage-only pass before incremental scrape
  --sync-gui      Run sync-token-from-gui.sh --force before checks
  --target NAME   Limit scrape/audit to one configured target
  --channel ID    With exactly one --target, limit scrape to channel ID (repeatable)
  --per-target    Scrape and audit each enabled target separately
  --continue-on-error  With --per-target, keep going after a target fails
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

enabled_targets() {
  jq -r '.targets[] | select(.enabled != false) | .name' "$CONFIG_PATH"
}

audit_targets() {
  local name failures=0
  if [[ -n "$TARGET" ]]; then
    run_step "audit-archive-json ($TARGET)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$TARGET"
    return
  fi
  local -a target_names=()
  mapfile -t target_names < <(enabled_targets)
  for name in "${target_names[@]}"; do
    [[ -n "$name" ]] || continue
    if run_step "audit-archive-json ($name)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$name"; then
      continue
    fi
    failures=$((failures + 1))
    if (( CONTINUE_ON_ERROR == 0 )); then
      return 1
    fi
  done
  (( failures == 0 ))
}

require_scrape_lock_free() {
  if ! ensure_scrape_lock_available "$CONFIG_PATH" "$LOCK_STATUS"; then
    die "Scrape lock is held; another scrape may be running. Inspect: $LOCK_STATUS --config $CONFIG_PATH"
  fi
}

run_documents_scrape() {
  local -a scrape_args=(--config "$CONFIG_PATH")
  scrape_args+=("${CHANNEL_ARGS[@]}")
  [[ -n "$TARGET" ]] && scrape_args+=(--target "$TARGET")

  if (( SALVAGE_ONLY )); then
    run_step "run-documents-scrape (salvage-only)" "$DOCUMENTS_SCRAPE" "${scrape_args[@]}" --salvage-only
    return $?
  fi

  if (( DRY_RUN )); then
    run_step "run-documents-scrape (dry-run)" "$DOCUMENTS_SCRAPE" "${scrape_args[@]}" --dry-run
    return $?
  fi

  if (( SALVAGE_BEFORE )); then
    run_step "run-documents-scrape (salvage-before)" "$DOCUMENTS_SCRAPE" "${scrape_args[@]}" --salvage-only || return $?
  fi

  run_step "run-documents-scrape" "$DOCUMENTS_SCRAPE" "${scrape_args[@]}"
}

scrape_per_target() {
  local name failures=0 ok=0
  local -a scrape_args=(--config "$CONFIG_PATH")
  scrape_args+=("${CHANNEL_ARGS[@]}")
  local -a target_names=()
  mapfile -t target_names < <(enabled_targets)
  for name in "${target_names[@]}"; do
    [[ -n "$name" ]] || continue
    log_step "Per-target begin: $name"
    local -a per_args=("${scrape_args[@]}" --target "$name")
    if (( SALVAGE_ONLY )); then
      if ! run_step "run-documents-scrape ($name salvage-only)" "$DOCUMENTS_SCRAPE" "${per_args[@]}" --salvage-only; then
        log_step "Per-target failed: $name (salvage-only)"
        failures=$((failures + 1))
        (( CONTINUE_ON_ERROR )) || return 1
        continue
      fi
      if run_step "audit-archive-json ($name)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$name"; then
        log_step "Per-target done: $name (salvage-only ok)"
        ok=$((ok + 1))
      else
        log_step "Per-target failed: $name (audit)"
        failures=$((failures + 1))
        (( CONTINUE_ON_ERROR )) || return 1
      fi
      continue
    fi
    if (( DRY_RUN )); then
      if ! run_step "run-documents-scrape ($name)" "$DOCUMENTS_SCRAPE" "${per_args[@]}" --dry-run; then
        log_step "Per-target failed: $name (dry-run)"
        failures=$((failures + 1))
        (( CONTINUE_ON_ERROR )) || return 1
        continue
      fi
      log_step "Per-target done: $name (dry-run)"
      ok=$((ok + 1))
      continue
    fi
    if (( SALVAGE_BEFORE )); then
      if ! run_step "run-documents-scrape ($name salvage)" "$DOCUMENTS_SCRAPE" "${per_args[@]}" --salvage-only; then
        log_step "Per-target failed: $name (salvage-before)"
        failures=$((failures + 1))
        (( CONTINUE_ON_ERROR )) || return 1
        continue
      fi
    fi
    if ! run_step "run-documents-scrape ($name)" "$DOCUMENTS_SCRAPE" "${per_args[@]}"; then
      log_step "Per-target failed: $name (scrape)"
      failures=$((failures + 1))
      if (( CONTINUE_ON_ERROR == 0 )); then
        return 1
      fi
      continue
    fi
    if (( DRY_RUN )); then
      log_step "Per-target done: $name (dry-run)"
      ok=$((ok + 1))
      continue
    fi
    if run_step "audit-archive-json ($name)" "$AUDIT_JSON" --config "$CONFIG_PATH" --target "$name"; then
      log_step "Per-target done: $name (ok)"
      ok=$((ok + 1))
    else
      log_step "Per-target failed: $name (audit)"
      failures=$((failures + 1))
      if (( CONTINUE_ON_ERROR == 0 )); then
        return 1
      fi
    fi
  done
  log_step "Per-target summary: $ok succeeded, $failures failed"
  (( failures == 0 ))
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
      --salvage-only)
        SALVAGE_ONLY=1
        shift
        ;;
      --salvage-before-scrape)
        SALVAGE_BEFORE=1
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
      --channel)
        [[ $# -ge 2 ]] || die "Missing value for --channel."
        CHANNEL_ARGS+=(--channel "$2")
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
      --per-target)
        PER_TARGET=1
        shift
        ;;
      --continue-on-error)
        CONTINUE_ON_ERROR=1
        shift
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

  if (( SALVAGE_ONLY == 1 && SALVAGE_BEFORE == 1 )); then
    die "--salvage-only and --salvage-before-scrape are mutually exclusive."
  fi
  if (( SALVAGE_ONLY == 1 && DRY_RUN == 1 )); then
    die "--salvage-only cannot be combined with --dry-run."
  fi

  mkdir -p "$LOG_DIR"
  if [[ -z "$LOG_FILE" ]]; then
    LOG_FILE="$LOG_DIR/operator-validation-$(date -u +%Y%m%dT%H%M%SZ).log"
  fi

  local export_json_summary=0
  if (( DRY_RUN == 0 && SKIP_SCRAPE == 0 && SALVAGE_ONLY == 0 )); then
    export_json_summary=1
    export DCE_RUN_SUMMARY_JSON=1
    if [[ -z "${DCE_RUN_SUMMARY_FILE:-}" ]]; then
      export DCE_RUN_SUMMARY_FILE="${LOG_FILE%.log}.summary.json"
    fi
  fi

  local failures=0

  set -o pipefail
  {
    log_step "Operator validation started (config=$CONFIG_PATH)"
    if [[ -n "$TARGET" ]]; then
      log_step "Targets: $TARGET"
      ((${#CHANNEL_ARGS[@]})) && log_step "Scrape channel filter: ${CHANNEL_ARGS[*]}"
      (( SALVAGE_ONLY )) && log_step "Mode: salvage-only"
      (( SALVAGE_BEFORE )) && log_step "Mode: salvage-before-scrape"
    else
      log_step "Enabled targets: $(enabled_targets | paste -sd, -)"
    fi
    if (( export_json_summary )); then
      log_step "JSON summary file: ${DCE_RUN_SUMMARY_FILE:-}"
    fi
    if (( SYNC_GUI_FLAG )); then
      run_step "sync-token-from-gui" "$SYNC_GUI" --force || failures=$((failures + 1))
    fi

    run_step "verify-operator-ready" "$VERIFY_READY" --config "$CONFIG_PATH" || failures=$((failures + 1))

    if (( SKIP_SCRAPE )); then
      log_step "Skip scrape requested."
    else
      require_scrape_lock_free || failures=$((failures + 1))
      if (( failures == 0 )); then
        if (( PER_TARGET )) && [[ -z "$TARGET" ]]; then
          scrape_per_target || failures=$((failures + 1))
        else
          if run_documents_scrape; then
            if (( DRY_RUN == 0 && failures == 0 )); then
              audit_targets || failures=$((failures + 1))
            fi
          else
            failures=$((failures + 1))
          fi
        fi
      fi
    fi

    if (( failures > 0 )); then
      log_step "Operator validation failed ($failures step(s))."
      exit 1
    fi
    log_step "Operator validation finished successfully."
  } 2>&1 | tee -a "$LOG_FILE"
  local pipeline_status=${PIPESTATUS[0]}

  if (( export_json_summary )) && [[ -n "${DCE_RUN_SUMMARY_FILE:-}" ]]; then
    if [[ ! -s "${DCE_RUN_SUMMARY_FILE}" ]]; then
      # shellcheck source=lib/scrape-summary-json.sh
      source "$SCRIPT_DIR/lib/scrape-summary-json.sh"
      if extract_json_summary_from_log "$LOG_FILE" "$DCE_RUN_SUMMARY_FILE"; then
        printf 'JSON summary recovered from log: %s\n' "$DCE_RUN_SUMMARY_FILE"
      fi
    fi
  fi

  printf 'Log: %s\n' "$LOG_FILE"
  exit "$pipeline_status"
}

main "$@"
