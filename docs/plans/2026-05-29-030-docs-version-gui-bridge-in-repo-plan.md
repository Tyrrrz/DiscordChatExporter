---
title: docs: Version GUI zip bridge doc in source repo
type: docs
status: complete
date: 2026-05-29
origin: /lfg — RECURRING-SCRAPE.md lived only in sibling GUI zip; track canonical copy in git
---

# docs: Version GUI zip bridge doc in source repo

## Summary

Add `docs/gui-zip-recurring-scrape-bridge.md` as the git-tracked canonical bridge for GUI zip users. Cross-link from README, operator checklist, and merge-readiness.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docs/gui-zip-recurring-scrape-bridge.md` with operator-handoff quick path (repo-relative paths) |
| R2 | `Readme.md` and operator checklist link to bridge doc |
| R3 | `docs/recurring-scrape-merge-readiness.md` notes GUI zip users read bridge doc |
| R4 | `run-all-smokes.sh` still passes |

## Verification

- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
