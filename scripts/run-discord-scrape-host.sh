#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)
REPO_ROOT="${DCE_REPO_ROOT:-$(cd "$SCRIPT_DIR/.." && pwd -P)}"
COMPOSE_FILE="${DCE_COMPOSE_FILE:-$REPO_ROOT/docker-compose.yml}"
ENV_FILE="${DCE_ENV_FILE:-$REPO_ROOT/scrape.env}"
DOCKER_BIN="${DCE_DOCKER_BIN:-docker}"
COMPOSE_BIN="${DCE_COMPOSE_BIN:-}"
DOCKER_BIN_OVERRIDDEN=0
REAUTH_COMMAND=""

if [[ -n "${DCE_DOCKER_BIN:-}" ]]; then
  DOCKER_BIN_OVERRIDDEN=1
fi

usage() {
  cat <<EOF
Usage:
  $(basename "$0") preflight [run-discord-scrape options...]
  $(basename "$0") scrape [run-discord-scrape options...]

Options:
  --env-file PATH      Env file to load and pass to compose. Default: $ENV_FILE
  --compose-file PATH  Compose file path. Default: $COMPOSE_FILE
  --help               Show this help text.

Environment:
  DISCORD_TOKEN            Direct token value (highest precedence after refresh).
  DISCORD_TOKEN_FILE       Optional path to a file containing the Discord token.
  DCE_REAUTH_COMMAND       Optional command to run for interactive/manual reauth when auth fails.
EOF
}

die() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

require_program() {
  command -v "$1" >/dev/null 2>&1 || die "Required command '$1' is missing."
}

load_env_file() {
  [[ -f "$ENV_FILE" ]] || die "Missing env file: $ENV_FILE"
  set -a
  # shellcheck disable=SC1090
  source "$ENV_FILE"
  set +a
}

load_token_from_file() {
  local token_file=${DISCORD_TOKEN_FILE:-}
  [[ -n "$token_file" ]] || return 1
  [[ -f "$token_file" ]] || die "DISCORD_TOKEN_FILE does not exist: $token_file"

  local token_value
  token_value=$(head -n 1 "$token_file" | tr -d '\r')
  [[ -n "$token_value" ]] || die "DISCORD_TOKEN_FILE is empty: $token_file"
  export DISCORD_TOKEN="$token_value"
  return 0
}

ensure_token_present() {
  if [[ -z "${DISCORD_TOKEN:-}" ]]; then
    load_token_from_file || true
  fi
  [[ -n "${DISCORD_TOKEN:-}" ]] || die "DISCORD_TOKEN is not set. Set DISCORD_TOKEN or DISCORD_TOKEN_FILE in $ENV_FILE."
}

compose_run_command() {
  local subcommand=$1
  shift
  local -a command_parts

  if [[ -n "$COMPOSE_BIN" ]]; then
    command_parts=(
      "$COMPOSE_BIN"
      --env-file "$ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  elif (( DOCKER_BIN_OVERRIDDEN == 0 )) && command -v docker-compose >/dev/null 2>&1; then
    command_parts=(
      docker-compose
      --env-file "$ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  else
    command_parts=(
      "$DOCKER_BIN"
      compose
      --env-file "$ENV_FILE"
      -f "$COMPOSE_FILE"
      run
      -T
      --rm
      discord-scraper
      "$subcommand"
    )
  fi

  command_parts+=("$@")
  printf '%q ' "${command_parts[@]}"
}

is_discord_auth_failure() {
  local output_file=$1
  grep -Eqi \
    "Authentication token is invalid|Request to 'channels/.+' failed: forbidden|failed authenticated preflight|401|403" \
    "$output_file"
}

try_interactive_reauth() {
  [[ -n "$REAUTH_COMMAND" ]] || return 1
  [[ -t 0 && -t 1 ]] || return 1
  printf 'Auth failed; running DCE_REAUTH_COMMAND...\n' >&2
  bash -lc "$REAUTH_COMMAND"
}

run_subcommand_with_retry() {
  local subcommand=$1
  shift
  local run_command output_file

  ensure_token_present
  output_file=$(mktemp "${TMPDIR:-/tmp}/dce-host-run.XXXXXX.log")

  run_command=$(compose_run_command "$subcommand" "$@")
  if eval "$run_command" >"$output_file" 2>&1; then
    cat "$output_file"
    rm -f "$output_file"
    return 0
  fi

  cat "$output_file" >&2

  if ! is_discord_auth_failure "$output_file"; then
    rm -f "$output_file"
    die "Container run failed for '$subcommand' with a non-auth error."
  fi

  printf 'Detected Discord auth failure. Refreshing token and retrying once...\n' >&2
  load_token_from_file || true
  try_interactive_reauth || true
  ensure_token_present

  if eval "$run_command" >"$output_file" 2>&1; then
    cat "$output_file"
    rm -f "$output_file"
    return 0
  fi

  cat "$output_file" >&2
  rm -f "$output_file"
  die "Container run failed for '$subcommand' after one auth refresh retry."
}

main() {
  local -a passthrough_args=()
  local subcommand=""

  while (($#)); do
    case "$1" in
      --env-file)
        [[ $# -ge 2 ]] || die "Missing value for --env-file."
        ENV_FILE=$2
        shift 2
        ;;
      --compose-file)
        [[ $# -ge 2 ]] || die "Missing value for --compose-file."
        COMPOSE_FILE=$2
        shift 2
        ;;
      --help|-h)
        usage
        exit 0
        ;;
      preflight|scrape)
        if [[ -n "$subcommand" ]]; then
          passthrough_args+=("$1")
        else
          subcommand=$1
        fi
        shift
        ;;
      *)
        if [[ -z "$subcommand" ]]; then
          die "Unsupported subcommand '$1'. Use 'preflight' or 'scrape'."
        fi
        passthrough_args+=("$1")
        shift
        ;;
    esac
  done

  [[ -n "$subcommand" ]] || {
    usage
    exit 1
  }

  require_program grep
  if [[ -n "$COMPOSE_BIN" ]]; then
    require_program "$COMPOSE_BIN"
  elif (( DOCKER_BIN_OVERRIDDEN == 0 )) && command -v docker-compose >/dev/null 2>&1; then
    :
  else
    require_program "$DOCKER_BIN"
  fi

  [[ -f "$COMPOSE_FILE" ]] || die "Missing compose file: $COMPOSE_FILE"
  load_env_file
  REAUTH_COMMAND="${DCE_REAUTH_COMMAND:-}"

  case "$subcommand" in
    preflight|scrape)
      run_subcommand_with_retry "$subcommand" "${passthrough_args[@]}"
      ;;
  esac
}

main "$@"
