---
title: "docs: Complete smoke test inventory"
type: docs
status: complete
date: 2026-06-04
origin: /lfg — plan 060 deferred `.docs/Recurring-Scrape-Setup.md` smoke table; 6 smokes missing after plans 054–061
---

# docs: Complete smoke test inventory

## Summary

Bring `.docs/Recurring-Scrape-Setup.md` smoke table in line with `scripts/run-all-smokes.sh` (21 offline scripts). Record plan 061 in merge-readiness.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Smoke table lists all 21 offline `scripts/tests/*.sh` except skipped `container-smoke.sh` |
| R2 | Table documents `container-smoke.sh` as local-only via `--include-container` |
| R3 | Section prose states 21 offline smokes match CI job `recurring-scrape-smoke` |
| R4 | `docs/recurring-scrape-merge-readiness.md` notes plan 061 (shared scrape-lock lib) |
| R5 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes (21/21) |

## Implementation Units

### U1. Setup doc smoke table

**Files:** `.docs/Recurring-Scrape-Setup.md`

Add missing rows:

- `archive-disk-space-smoke.sh`
- `run-discord-scrape-host-lock-smoke.sh`
- `operator-handoff-smoke.sh`
- `run-operator-proof-smoke.sh`
- `scrape-lock-status-smoke.sh`
- `sync-gui-bridge-doc-smoke.sh`

### U2. Merge-readiness stamp

**Files:** `docs/recurring-scrape-merge-readiness.md`

Add plan 061 bullet under branch status / latest section.

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Container memory tuning for large channels
