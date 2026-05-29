#!/usr/bin/env bash
# Run recurring append-only scrape tooling from the source repository root.
set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd -P)
exec "$REPO_ROOT/scripts/run-discord-scrape-host.sh" "$@"
