---
title: "refactor: Shared scrape-lock library"
type: refactor
status: active
date: 2026-06-04
origin: /lfg — lock path and gate logic duplicated across host runner, status script, validation, documents scrape
---

# refactor: Shared scrape-lock library

## Summary

Extract `scripts/lib/scrape-lock.sh` and source it from lock-related scripts to keep archive-root lock behavior consistent.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `lib/scrape-lock.sh` provides resolve, held check, holder formatting, reclaim helpers |
| R2 | `scrape-lock-status.sh` and `run-discord-scrape-host.sh` source the library |
| R3 | `run-documents-scrape.sh` and `run-operator-validation.sh` use shared `ensure_scrape_lock_available` |
| R4 | `run-all-smokes.sh` passes (21 smokes) |

## Implementation Units

### U1. Library extraction

**Files:** `scripts/lib/scrape-lock.sh`, consumers listed above

### U2. Smoke gate

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
