---
title: "feat: Per-target JSON summaries in multi-target loops"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 075 deferred per-target separate summary files in validation/proof loops
---

# feat: Per-target JSON summaries in multi-target loops

## Summary

When operator validation runs `--per-target` (all enabled targets) or operator proof scrapes multiple targets, pass `--summary-file` per target so each scrape writes `logs/<prefix>-<target>-<UTC>.summary.json` instead of overwriting a single combined path.

## Problem Frame

Plans 070–075 auto-export JSON summaries for single-target and documents-scrape runs. Multi-target loops still set one global `DCE_RUN_SUMMARY_FILE` tied to the teed log basename — only the last target's scrape wins on disk, and recovery from the combined log cannot disambiguate targets.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scripts/lib/scrape-summary-json.sh` exposes `per_target_summary_file LOG_DIR PREFIX TARGET` with sanitized target slug |
| R2 | `run-operator-validation.sh --per-target` (no `--target`) skips global `DCE_RUN_SUMMARY_FILE`; each live scrape passes `--summary-file` |
| R3 | Validation logs `Per-target JSON summary: <path>` before each live scrape |
| R4 | `run-operator-proof.sh` with 2+ targets uses per-target `--summary-file`; single-target keeps log-basename summary |
| R5 | Proof logs per-target summary path in the target loop when exporting JSON |
| R6 | End-of-run log recovery skipped when per-target mode (files written directly by scrape) |
| R7 | `scrape-summary-json-smoke.sh` asserts helper output shape |
| R8 | `run-operator-validation-smoke.sh` multi-target dry-run still passes; optional fake-docker live per-target asserts two distinct `--summary-file` paths in subprocess output |
| R9 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. Shared helper

**Files:** `scripts/lib/scrape-summary-json.sh`, `scripts/tests/scrape-summary-json-smoke.sh`

### U2. Operator validation

**Files:** `scripts/run-operator-validation.sh`

### U3. Operator proof

**Files:** `scripts/run-operator-proof.sh`

### U4. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Tee full documents-scrape stdout to persistent log
- Refresh PR #1538 body with plans 070–076 stamps
