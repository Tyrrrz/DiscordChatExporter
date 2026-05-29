---
title: feat: Single operator handoff verification script
type: feat
status: complete
date: 2026-05-29
origin: /lfg — recurring scrape stack complete; one command for pre-cron verification
---

# feat: Single operator handoff verification script

## Summary

Provide `./scripts/operator-handoff.sh` that runs the canonical offline checks (disk, archives, optional dry-run) and prints a clear pass/fail summary before monthly cron or a full scrape.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Script runs `verify-operator-ready.sh` (full check, not only disk) |
| R2 | Script runs `run-documents-scrape.sh --dry-run` |
| R3 | Prints `df` for archive_root and repo root |
| R4 | `--skip-df` and `--config PATH` supported |
| R5 | `operator-handoff-smoke.sh` validates exit 0 with fixture config |
| R6 | Operator checklist links to the script |

## Verification

- `./scripts/tests/operator-handoff-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
