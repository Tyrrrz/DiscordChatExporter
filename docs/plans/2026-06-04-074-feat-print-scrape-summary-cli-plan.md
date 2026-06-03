---
title: "feat: print-scrape-summary CLI for JSON run artifacts"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 073 deferred print-scrape-summary.sh to pretty-print machine-readable scrape totals
---

# feat: print-scrape-summary CLI for JSON run artifacts

## Summary

Add `scripts/print-scrape-summary.sh` to render human-readable scrape totals from `*.summary.json` files produced by validation, proof, and host runs.

## Problem Frame

Plans 069–073 emit JSON summaries beside operator logs. Operators still need `jq` one-liners to inspect OOM skips, per-channel deltas, and appended message counts. A small read-only CLI closes the loop without opening raw JSON.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Accept summary file path argument; read stdin when path is `-` |
| R2 | Validate `version == 1` and required `totals` fields with `jq` |
| R3 | Default output: finished_at, totals line, and per-channel table (ACTION, CHANNEL, LABEL, DELTA, FILE) |
| R4 | `--json` prints raw file unchanged |
| R5 | `--oom-only` lists only channels whose action is `SKIPPED_OOM` |
| R6 | Exit non-zero on missing/invalid file |
| R7 | Offline smoke with fixture JSON; `run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. print-scrape-summary.sh

**Files:** `scripts/print-scrape-summary.sh`, `scripts/tests/print-scrape-summary-smoke.sh`

**Approach:** jq for parsing/formatting; match action labels from `run-discord-scrape.sh` summary text where practical.

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`

**Approach:** Plan 074 stamp; one-line usage beside summary.json examples.

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target separate summary files in multi-target proof loops
- Auto-enable JSON summary on bare `host.sh scrape` without env vars
