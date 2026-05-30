---
title: feat: Complete interrupted full-target validation
type: feat
status: complete
date: 2026-05-30
origin: /lfg — 2026-05-30 validation stalled on KotOR_discord_msgs; finish remaining targets
---

# feat: Complete interrupted full-target validation

## Summary

The 2026-05-30 `full-validation-latest.log` run completed 4/9 targets then stalled during `KotOR_discord_msgs` scrape. Resume validation per remaining target, fix misleading per-target log labels, and finalize merge-readiness table.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Run `run-operator-validation.sh --target NAME` for each incomplete enabled target; append to `logs/validation-resume-20260530.log` |
| R2 | `docs/recurring-scrape-merge-readiness.md` table shows final pass/fail per target |
| R3 | `run-operator-validation.sh` logs `Per-target begin` / `Per-target done` instead of premature `Per-target pass` |
| R4 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes |
| R5 | PR #1538 updated |

## Verification

- `grep 'Operator validation finished' logs/validation-resume-20260530.log` (per target)
- 19 offline smokes
