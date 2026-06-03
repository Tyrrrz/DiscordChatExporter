---
title: "feat: Documents scrape --log-file with tee"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 077 deferred tee full documents-scrape stdout to persistent log
---

# feat: Documents scrape --log-file with tee

## Summary

Add `--log-file PATH` to `run-documents-scrape.sh`. Live scrapes auto-tee to `logs/documents-scrape-<UTC>.log` and pair JSON summary with `<log-basename>.summary.json` (parity with operator validation).

## Problem Frame

Validation and proof persist teed logs with recoverable JSON summaries. The primary cron/operator entry `run-documents-scrape.sh` only prints to stdout; long KotOR catch-up runs leave no durable log unless the operator wraps the command manually.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `--log-file PATH` appends all workflow output via `tee -a` |
| R2 | Live scrape (not dry-run/salvage-only) auto-defaults log to `logs/documents-scrape-<UTC>.log` when unset |
| R3 | Live scrape pairs summary with `${LOG_FILE%.log}.summary.json` unless `--summary-file` or `DCE_RUN_SUMMARY_FILE` set |
| R4 | Prints `Log file:` before scrape; `Log:` after tee completes |
| R5 | Recovers missing summary from teed log via `recover_json_summary_if_missing` |
| R6 | Dry-run and salvage-only skip auto log/summary unless `--log-file` explicitly passed |
| R7 | `documents-scrape-smoke.sh` asserts teed log file on live `--log-file` run |
| R8 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. run-documents-scrape.sh

**Files:** `scripts/run-documents-scrape.sh`, `scripts/tests/documents-scrape-smoke.sh`

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`, `scrape.env.example`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Refresh PR #1538 body with plans 070–078 stamps
- Wire `--log-file` into setup-cron crontab line
