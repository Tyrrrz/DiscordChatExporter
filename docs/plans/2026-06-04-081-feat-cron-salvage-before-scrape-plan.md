---
title: "feat: Cron opt-in salvage-before-scrape"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 080 deferred --salvage-before-scrape on cron installs
---

# feat: Cron opt-in salvage-before-scrape

## Summary

Add `--salvage-before-scrape` to `setup-cron.sh` so scheduled jobs can merge stale `.dce-temp` exports before incremental scrape (recommended for KotOR catch-up after OOM).

## Problem Frame

Operators use `--salvage-before-scrape` manually on documents scrape and validation; monthly cron (plan 079) runs plain documents scrape without salvage, leaving partial temps unmerged until a manual pass.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `setup-cron.sh --salvage-before-scrape` appends flag to documents scrape cron command |
| R2 | Default install unchanged (no salvage unless flag passed) |
| R3 | Usage and examples document the flag |
| R4 | `setup-cron-smoke.sh` dry-run asserts flag in preview when passed |
| R5 | Docs note KotOR/cron salvage opt-in |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. setup-cron.sh

**Files:** `scripts/setup-cron.sh`, `scripts/tests/setup-cron-smoke.sh`

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`, `.docs/Recurring-Scrape-Setup.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Prune stale duplicate Latest blocks from PR body
