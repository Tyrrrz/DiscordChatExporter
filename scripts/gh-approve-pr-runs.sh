#!/usr/bin/env bash

set -Eeuo pipefail

GH_BIN="${GH_BIN:-gh}"
BASHRC="${GH_APPROVE_BASHRC:-${HOME}/.bashrc}"
REPO_SPEC=""
declare -a RUN_IDS=()

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --repo OWNER/NAME RUN_ID [RUN_ID...]

Attempt to approve GitHub Actions workflow runs (for example, fork PR runs
waiting on maintainer approval). Bootstraps gh auth from GITHUB_TOKEN when needed.

Options:
  --repo OWNER/NAME   Repository containing the workflow runs (required)
  --help              Show this help text

Environment:
  GITHUB_TOKEN        Personal access token with actions:write (or repo admin)
  GH_BIN              gh executable (default: gh)
  GH_APPROVE_BASHRC   Shell rc file to source for token bootstrap (default: ~/.bashrc)
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_program() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

maybe_source_bashrc() {
  [[ -f "$BASHRC" ]] || return 0
  # shellcheck disable=SC1090
  source "$BASHRC"
}

ensure_gh_auth() {
  [[ -n "${GITHUB_TOKEN:-}" ]] || die "GITHUB_TOKEN is not set. Export a token or define it in $BASHRC."

  if ! "$GH_BIN" auth status >/dev/null 2>&1; then
    printf 'GitHub CLI is not authenticated; logging in from GITHUB_TOKEN...\n' >&2
    printf '%s\n' "$GITHUB_TOKEN" | "$GH_BIN" auth login --with-token \
      || die "gh auth login --with-token failed."
  fi

  "$GH_BIN" auth status >/dev/null 2>&1 || die "GitHub CLI authentication is still invalid after token login."
}

classify_approve_failure() {
  local output=$1

  if grep -Eqi 'must have admin rights|admin rights to this repository|Resource not accessible by integration' <<<"$output"; then
    printf 'POLICY_BLOCKER: GitHub requires repository admin rights to approve this workflow run. This is an upstream permission/policy limit, not a transient auth failure.\n' >&2
    return 2
  fi

  if grep -Eqi 'Bad credentials|HTTP 401|HTTP 403' <<<"$output"; then
    printf 'AUTH_FAILURE: GitHub rejected the approval request. Verify GITHUB_TOKEN scopes and gh auth status.\n' >&2
    return 1
  fi

  return 1
}

approve_run() {
  local repo=$1 run_id=$2
  local output

  if output=$("$GH_BIN" api -X POST "repos/${repo}/actions/runs/${run_id}/approve" 2>&1); then
    printf 'Approved workflow run %s on %s.\n' "$run_id" "$repo"
    return 0
  fi

  classify_approve_failure "$output"
  local classify_rc=$?
  printf '%s\n' "$output" >&2
  return "$classify_rc"
}

main() {
  while (($#)); do
    case "$1" in
      --repo)
        [[ $# -ge 2 ]] || die "Missing value for --repo."
        REPO_SPEC=$2
        shift 2
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      -*)
        die "Unknown option: $1"
        ;;
      *)
        RUN_IDS+=("$1")
        shift
        ;;
    esac
  done

  [[ -n "$REPO_SPEC" ]] || {
    usage
    exit 1
  }
  ((${#RUN_IDS[@]} > 0)) || die "Provide at least one workflow RUN_ID."

  [[ "$REPO_SPEC" == */* ]] || die "--repo must use OWNER/NAME format."

  require_program "$GH_BIN"
  maybe_source_bashrc
  ensure_gh_auth

  local run_id failures=0 policy_blockers=0
  for run_id in "${RUN_IDS[@]}"; do
    if approve_run "$REPO_SPEC" "$run_id"; then
      continue
    fi
    local rc=$?
    failures=$((failures + 1))
    if (( rc == 2 )); then
      policy_blockers=$((policy_blockers + 1))
    fi
  done

  if (( policy_blockers > 0 )); then
    die "One or more runs could not be approved because the authenticated user lacks required repository admin rights."
  fi

  if (( failures > 0 )); then
    die "Failed to approve ${failures} workflow run(s)."
  fi
}

main "$@"
