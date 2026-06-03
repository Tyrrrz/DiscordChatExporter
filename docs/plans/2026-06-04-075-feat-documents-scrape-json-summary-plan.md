---
title: "feat: Auto JSON summary on documents scrape"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 074 deferred auto-enable JSON summary on bare scrape entrypoints; cron uses run-documents-scrape.sh
---

# feat: Auto JSON summary on documents scrape

## Summary

When `run-documents-scrape.sh` performs a live Discord scrape, auto-enable `DCE_RUN_SUMMARY_JSON=1` and write `logs/documents-scrape-<UTC>.summary.json` unless the operator already set `DCE_RUN_SUMMARY_FILE` or passes `--summary-file`.

## Problem Frame

Validation and proof auto-export JSON summaries (plans 070–073). The primary incremental path — `run-documents-scrape.sh` and monthly cron — still requires manual env vars for machine-readable totals. Host runner recovery (plan 072) can populate the file from compose logs when env is set.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Live scrape path exports `DCE_RUN_SUMMARY_JSON=1` when not dry-run/salvage-only |
| R2 | Default `DCE_RUN_SUMMARY_FILE` to `logs/documents-scrape-<UTC>.summary.json` when unset |
| R3 | Optional `--summary-file PATH` overrides default destination |
| R4 | Prints `JSON summary file:` before preflight/scrape |
| R5 | Dry-run and salvage-only do not enable JSON export |
| R6 | `documents-scrape-smoke.sh` asserts summary path on live scrape and absence on dry-run |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. run-documents-scrape.sh

**Files:** `scripts/run-documents-scrape.sh`, `scripts/tests/documents-scrape-smoke.sh`

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `scrape.env.example`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target separate summary files in multi-target proof/validation loops
- Tee full documents-scrape stdout to a log file
