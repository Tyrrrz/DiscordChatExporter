---
title: docs: Merge readiness index and doc cross-links
type: docs
status: complete
date: 2026-05-29
origin: Repeated /lfg — feature stack complete; surface merge/operator entrypoints
---

# docs: Merge readiness index and doc cross-links

## Summary

Recurring scrape automation is implemented and tested. Add a merge-readiness doc for reviewers and wire documentation indexes so operators find setup, troubleshooting, and validation in one hop.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docs/recurring-scrape-merge-readiness.md` summarizes feature, validation commands, operator flow |
| R2 | `.docs/Readme.md` links recurring scrape setup and troubleshooting |
| R3 | Root `Readme.md` See also mentions `run-all-smokes.sh` validation |

## Verification

- `./scripts/run-all-smokes.sh` passes
