---
title: docs: LFG merge-ready stamp
type: docs
status: complete
date: 2026-05-29
origin: /lfg — recurring scrape validated on host; document merge-ready state
---

# docs: LFG merge-ready stamp

## Summary

Recurring scrape is validated offline (19 smokes), live (`run-operator-proof` on `eod_discord`), and scheduled (monthly cron installed). Stamp merge-readiness and operator checklist accordingly.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docs/recurring-scrape-merge-readiness.md` has explicit merge-ready / host-activation status |
| R2 | `docs/recurring-scrape-operator-checklist.md` reflects post-live-proof path |
| R3 | `sync-gui-bridge-doc.sh` run; 19 smokes pass |
| R4 | PR #1538 updated with plan 036 closure |

## Verification

- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
- `./scripts/setup-cron.sh --dry-run` (preflight all enabled targets)
