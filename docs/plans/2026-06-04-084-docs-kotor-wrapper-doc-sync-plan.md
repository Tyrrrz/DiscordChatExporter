---
title: "docs: sync KotOR wrapper across GUI bridge and merge-readiness"
type: docs
status: complete
date: 2026-06-04
origin: /lfg — plan 083 landed wrapper; GUI bridge and merge-readiness header still stale
---

# docs: sync KotOR wrapper across GUI bridge and merge-readiness

## Summary

Update `docs/gui-zip-recurring-scrape-bridge.md`, `.docs/Recurring-Scrape-Troubleshooting.md`, and `docs/recurring-scrape-merge-readiness.md` so operators and GUI-zip users see `run-kotor-yes-general-catchup.sh` as the primary yes_general path and **24/24** smoke gate everywhere.

## Problem Frame

Plan 083 added the KotOR catch-up wrapper and updated setup/checklist docs. The GUI bridge doc still documents long manual validation commands and claims **21** offline smokes. Merge-readiness branch HEAD stamp still references `18a22a6` instead of current `7171d7b`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | GUI bridge yes_general section leads with `./scripts/run-kotor-yes-general-catchup.sh` |
| R2 | GUI bridge smoke line reads **24/24** (optional `--include-container`) |
| R3 | Troubleshooting OOM section cites wrapper + `print-scrape-summary.sh` |
| R4 | Merge-readiness table HEAD → `7171d7b` (or post-commit SHA) |
| R5 | `sync-gui-bridge-doc-smoke.sh` asserts dest mentions wrapper |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 24/24 |

## Implementation Units

### U1. Doc updates

**Files:** `docs/gui-zip-recurring-scrape-bridge.md`, `.docs/Recurring-Scrape-Troubleshooting.md`, `docs/recurring-scrape-merge-readiness.md`

### U2. Bridge sync smoke

**Files:** `scripts/tests/sync-gui-bridge-doc-smoke.sh`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on operator host
- PR #1538 body refresh (already at plans 070–083)
