---
title: "feat: Operator proof JSON scrape summary export"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 072 deferred operator-proof JSON summary parity with validation
---

# feat: Operator proof JSON scrape summary export

## Summary

Mirror operator-validation JSON summary behavior in `run-operator-proof.sh`: auto-enable `DCE_RUN_SUMMARY_*` when scraping, write `*.summary.json` beside the proof log, and recover from the teed log when the file is missing.

## Problem Frame

Validation and host runner now emit machine-readable scrape totals (plans 069–072). `run-operator-proof.sh` is the one-target handoff → scrape → prove path but still only produces human-readable logs. Operators running KotOR yes_general proof cannot get `.summary.json` without switching to validation.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Optional `--log-file PATH`; default `logs/operator-proof-<UTC>.log` |
| R2 | When scraping (not dry-run, not salvage-only), export `DCE_RUN_SUMMARY_JSON=1` and `*.summary.json` beside log |
| R3 | Log `JSON summary file:` at proof start when export enabled |
| R4 | After tee, recover summary via `recover_json_summary_if_missing` when file missing |
| R5 | Dry-run and salvage-only do not enable JSON export |
| R6 | `run-operator-proof-smoke.sh` asserts dry-run skips JSON summary |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 22/22 |

## Implementation Units

### U1. Operator proof wiring

**Files:** `scripts/run-operator-proof.sh`, `scripts/tests/run-operator-proof-smoke.sh`

**Approach:** Match validation pattern; export env before tee; recovery after tee.

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`

**Approach:** Plan 073 stamp; note `.summary.json` beside proof logs.

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target separate summary files in multi-target proof loops
- `print-scrape-summary.sh` CLI to pretty-print JSON
