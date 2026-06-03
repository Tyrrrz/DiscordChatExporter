#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
SYNC="$REPO_ROOT/scripts/sync-gui-bridge-doc.sh"
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-bridge-sync-smoke.XXXXXX")
DEST="$TMP_DIR/gui-zip/RECURRING-SCRAPE.md"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$TMP_DIR/gui-zip"

"$SYNC" --dest "$DEST"
[[ -f "$DEST" ]] || { printf 'ERROR: dest missing\n' >&2; exit 1; }
grep -q 'operator-handoff' "$DEST" || { printf 'ERROR: dest content unexpected\n' >&2; exit 1; }
grep -q 'run-kotor-yes-general-catchup' "$DEST" || { printf 'ERROR: dest missing KotOR wrapper\n' >&2; exit 1; }
grep -q '24/24' "$DEST" || { printf 'ERROR: dest missing 24/24 smoke count\n' >&2; exit 1; }

printf 'sync-gui-bridge-doc-smoke: ok\n'
