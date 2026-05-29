#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
INCLUDE_CONTAINER=0

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [--include-container]

Run every script in scripts/tests/ (offline smokes by default).
Use --include-container to also run container-smoke.sh (needs Docker/Podman).
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

main() {
  while (($#)); do
    case "$1" in
      --include-container)
        INCLUDE_CONTAINER=1
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

  local tests_dir="$REPO_ROOT/scripts/tests"
  [[ -d "$tests_dir" ]] || die "Missing tests directory: $tests_dir"

  chmod +x "$REPO_ROOT"/scripts/*.sh "$tests_dir"/*.sh 2>/dev/null || true

  local script_path failures=0 ran=0
  for script_path in "$tests_dir"/*.sh; do
    [[ -f "$script_path" ]] || continue
    local base
    base=$(basename "$script_path")
    if [[ "$base" == "container-smoke.sh" && "$INCLUDE_CONTAINER" -eq 0 ]]; then
      printf 'SKIP: %s (pass --include-container to run)\n' "$base"
      continue
    fi
    printf '==> %s\n' "$base"
    if ! "$script_path"; then
      failures=$((failures + 1))
    fi
    ran=$((ran + 1))
  done

  if (( ran == 0 )); then
    die "No smoke scripts found under $tests_dir"
  fi

  if (( failures > 0 )); then
    die "$failures smoke script(s) failed."
  fi

  printf 'All %d smoke script(s) passed.\n' "$ran"
}

main "$@"
