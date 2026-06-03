---
title: "docs: Sync smoke inventory to 23 offline tests"
type: docs
status: complete
date: 2026-06-04
origin: /lfg — plan 062 deferred refresh; plans 071/074 added smokes not reflected in setup doc or merge-readiness header
---

# docs: Sync smoke inventory to 23 offline tests

## Summary

Update `.docs/Recurring-Scrape-Setup.md` and `docs/recurring-scrape-merge-readiness.md` so smoke counts and tables match `scripts/run-all-smokes.sh` (23 offline scripts; 24 with `--include-container`).

## Problem Frame

Plan 062 listed 21 smokes. Plans 071 (`scrape-summary-json-smoke.sh`) and 074 (`print-scrape-summary-smoke.sh`) landed without updating the setup doc table or the merge-readiness status table (still says 21/21).

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Setup doc prose says **23 offline smokes**; `--include-container` → 24th local-only |
| R2 | Smoke table includes `print-scrape-summary-smoke.sh` and `scrape-summary-json-smoke.sh` |
| R3 | Table rows remain alphabetically sorted |
| R4 | Merge-readiness branch status table shows **23/23** offline smokes |
| R5 | Merge-readiness adds plan 077 stamp |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. Setup doc

**Files:** `.docs/Recurring-Scrape-Setup.md`

### U2. Merge-readiness

**Files:** `docs/recurring-scrape-merge-readiness.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Tee full documents-scrape stdout to persistent log
- Refresh PR #1538 body with plans 070–077 stamps
