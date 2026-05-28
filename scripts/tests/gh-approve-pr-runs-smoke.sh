#!/usr/bin/env bash

set -Eeuo pipefail

REPO_ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd -P)
TMP_DIR=$(mktemp -d "${TMPDIR:-/tmp}/dce-gh-approve-smoke.XXXXXX")
FAKE_GH="$TMP_DIR/gh"
GH_LOG="$TMP_DIR/gh.log"
GH_STATE="$TMP_DIR/gh.state"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

cat >"$FAKE_GH" <<'EOF'
#!/usr/bin/env bash
set -Eeuo pipefail

log=${FAKE_GH_LOG:?}
mode=${FAKE_GH_MODE:?}
state=${FAKE_GH_STATE:?}

printf '%s\n' "$*" >>"$log"

case "$1" in
  auth)
    if [[ "${2:-}" == "login" ]]; then
      touch "$state"
      exit 0
    fi
    if [[ -f "$state" ]] || [[ "$mode" == "authenticated" ]]; then
      exit 0
    fi
    exit 1
    ;;
  api)
    if [[ "$mode" == "policy-blocker" ]]; then
      printf 'Must have admin rights to Repository.\n' >&2
      exit 1
    fi
    if [[ "$mode" == "auth-fail" ]]; then
      printf 'HTTP 401: Bad credentials\n' >&2
      exit 1
    fi
    exit 0
    ;;
  *)
    echo "unexpected gh invocation: $*" >&2
    exit 1
    ;;
esac
EOF
chmod +x "$FAKE_GH"

run_helper() {
  local mode=$1
  shift

  : >"$GH_LOG"
  rm -f "$GH_STATE"

  GITHUB_TOKEN=test-token \
  GH_BIN="$FAKE_GH" \
  GH_APPROVE_BASHRC=/dev/null \
  FAKE_GH_LOG="$GH_LOG" \
  FAKE_GH_MODE="$mode" \
  FAKE_GH_STATE="$GH_STATE" \
  "$REPO_ROOT/scripts/gh-approve-pr-runs.sh" "$@"
}

if run_helper authenticated --repo Tyrrrz/DiscordChatExporter 12345 67890; then
  grep -q 'api -X POST repos/Tyrrrz/DiscordChatExporter/actions/runs/12345/approve' "$GH_LOG" \
    || { echo "expected first run approval API call" >&2; exit 1; }
  grep -q 'api -X POST repos/Tyrrrz/DiscordChatExporter/actions/runs/67890/approve' "$GH_LOG" \
    || { echo "expected second run approval API call" >&2; exit 1; }
else
  echo "expected successful approval for authenticated mode" >&2
  exit 1
fi

if run_helper unauthenticated --repo Tyrrrz/DiscordChatExporter 11111; then
  grep -q 'auth login --with-token' "$GH_LOG" || { echo "expected gh auth login from token" >&2; exit 1; }
else
  echo "expected bootstrap + approval for unauthenticated gh" >&2
  exit 1
fi

if run_helper policy-blocker --repo Tyrrrz/DiscordChatExporter 22222 2>/tmp/gh-policy.err; then
  echo "expected policy blocker to fail" >&2
  exit 1
fi
grep -q 'POLICY_BLOCKER' /tmp/gh-policy.err || { echo "expected policy blocker classification" >&2; exit 1; }

if GITHUB_TOKEN= GH_BIN="$FAKE_GH" GH_APPROVE_BASHRC=/dev/null \
   "$REPO_ROOT/scripts/gh-approve-pr-runs.sh" --repo Tyrrrz/DiscordChatExporter 1 2>/tmp/gh-missing-token.err; then
  echo "expected missing token failure" >&2
  exit 1
fi
grep -q 'GITHUB_TOKEN is not set' /tmp/gh-missing-token.err || { echo "expected missing token message" >&2; exit 1; }

echo "gh-approve-pr-runs smoke test passed"
