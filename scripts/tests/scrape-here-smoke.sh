#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
SCRAPE_HERE="$REPO_ROOT/scripts/scrape-here.sh"

[[ -x "$SCRAPE_HERE" ]] || {
  printf 'scrape-here.sh is not executable\n' >&2
  exit 1
}

"$SCRAPE_HERE" --help | grep -q 'run-discord-scrape.sh' || {
  printf 'scrape-here --help did not show scrape subcommand help\n' >&2
  exit 1
}

if "$SCRAPE_HERE" not-a-subcommand 2>/dev/null; then
  printf 'expected failure for unknown subcommand\n' >&2
  exit 1
fi

printf 'scrape-here-smoke: ok\n'
