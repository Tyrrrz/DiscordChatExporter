---
title: feat: Full per-target operator validation run
type: feat
status: complete
date: 2026-05-29
origin: /lfg — all enabled targets scrape + audit after merge-ready stamp
---

# feat: Full per-target operator validation run

## Summary

Run `run-operator-validation.sh --sync-gui --per-target --continue-on-error` on the host and record pass/fail per target in merge-readiness. Re-run offline smokes before push.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Validation completes or stops with per-target summary in log |
| R2 | `docs/recurring-scrape-merge-readiness.md` lists per-target validation outcome |
| R3 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes |
| R4 | PR #1538 updated with validation summary |

## Verification

- `logs/operator-validation-*.log` or `logs/full-validation-latest.log`
- Offline smokes (19 scripts)
