---
title: "fix: Archive-root scrape lock with holder diagnostics"
type: fix
status: complete
date: 2026-06-04
origin: /lfg — stale KotOR validation runs from MyBook checkout while Downloads checkout has a separate `.dce-scrape.lock`; both can touch the same `~/Documents` archives
---

# fix: Archive-root scrape lock with holder diagnostics

## Summary

Move the host scrape flock from per-repo `.dce-scrape.lock` to `{archive_root}/.dce-scrape.lock` (from scrape config), write a sidecar `.meta` file with holder PID/command, and improve the lock-held error so operators know what is blocking a run.

## Problem Frame

Plan 046 added scrape serialization via `flock`, but the lock path defaults to `$REPO_ROOT/.dce-scrape.lock`. The operator has two checkouts (`~/Downloads/DiscordChatExporter` and `/run/media/.../MyBook/...`) sharing the same `archive_root` in `config/scrape-targets.json`. A long-running validation from the MyBook path does not block a new scrape from the Downloads path, risking twin exports and OOM loops on `yes_general`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Default lock file is `{archive_root}/.dce-scrape.lock` resolved from the host config used for the scrape |
| R2 | `DCE_SCRAPE_LOCK_FILE` override continues to work unchanged |
| R3 | On acquire, write `{lock}.meta` with pid, UTC started timestamp, and command summary |
| R4 | On lock failure, error cites meta (pid, started, cmd) when present |
| R5 | If meta pid is not running, reclaim lock automatically with a warning log |
| R6 | Release removes `.meta` alongside releasing flock |
| R7 | Lock smoke covers archive-root path; `run-all-smokes.sh` passes |

## Key Technical Decisions

- **Lock at archive_root**: Matches the shared resource being mutated (Documents archives), not the git checkout path.
- **Fallback**: If config lacks `archive_root`, keep `$REPO_ROOT/.dce-scrape.lock` fallback for tests/minimal configs.
- **Reclaim only when pid dead**: Do not force-break a live holder; kernel releases flock when the holder exits.

## Implementation Units

### U1. Resolve lock path and meta lifecycle

**Goal:** Host runner acquires archive-root lock with meta sidecar.

**Requirements:** R1–R6

**Files:**
- `scripts/run-discord-scrape-host.sh`

**Approach:** Add `resolve_scrape_lock_file`, `write_scrape_lock_meta`, `format_scrape_lock_holder`, `try_reclaim_stale_scrape_lock`; pass `host_config` into `acquire_scrape_lock`; call from scrape branch after config resolution.

**Test scenarios:**
- Config with `archive_root=/tmp/x` uses `/tmp/x/.dce-scrape.lock` when override unset.
- `DCE_SCRAPE_LOCK_FILE` still wins over archive_root.
- Dead pid in meta allows second acquire after reclaim.

**Verification:** Lock smoke passes.

### U2. Extend lock smoke

**Goal:** Regression for archive-root default and informative lock-held message.

**Requirements:** R7

**Files:**
- `scripts/tests/run-discord-scrape-host-lock-smoke.sh`

**Test scenarios:**
- Two processes: first holds flock on `{archive_root}/.dce-scrape.lock`, second fails with holder hint.
- After killing holder, second scrape succeeds.

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`

## Scope Boundaries

### In scope

- Lock path, meta, reclaim, smoke

### Deferred to Follow-Up Work

- Killing the stale MyBook validation process on the host
- Live yes_general channel catch-up inside LFG
- Container memory limits
