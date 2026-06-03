---
title: "feat: Scrape lock status diagnostic"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 053 moved lock to archive_root; operators need read-only visibility before starting validation or killing stale runs
---

# feat: Scrape lock status diagnostic

## Summary

Add `scripts/scrape-lock-status.sh` to report archive-root scrape lock state (path, holder pid/cmd/started, live vs stale) and call it from `operator-handoff.sh` so handoff surfaces blocking scrapes.

## Problem Frame

Two checkouts can share `~/Documents` archives. A long validation holds `{archive_root}/.dce-scrape.lock` but operators only discover it when a second scrape fails. They need a read-only check before starting work.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scrape-lock-status.sh --config PATH` prints lock file path and state |
| R2 | Resolves lock via `DCE_SCRAPE_LOCK_FILE` or `{archive_root}/.dce-scrape.lock` (same rules as host runner) |
| R3 | Reads `.meta` sidecar when present (pid, started, cmd) |
| R4 | Exit 0 when safe to scrape (free or stale reclaimable); exit 1 when actively held |
| R5 | `operator-handoff.sh` prints lock status section after verify-operator-ready |
| R6 | Offline smoke covers held, free, and archive-root path; `run-all-smokes.sh` passes |

## Implementation Units

### U1. scrape-lock-status.sh

**Files:** `scripts/scrape-lock-status.sh`

### U2. Operator handoff integration

**Files:** `scripts/operator-handoff.sh`, `scripts/tests/operator-handoff-smoke.sh`

### U3. Lock status smoke

**Files:** `scripts/tests/scrape-lock-status-smoke.sh`

## Scope Boundaries

### Deferred

- Refactoring host runner to shared lib (duplicate minimal resolve logic in status script)
- Live KotOR catch-up on host
- operator-handoff `--salvage-only`
