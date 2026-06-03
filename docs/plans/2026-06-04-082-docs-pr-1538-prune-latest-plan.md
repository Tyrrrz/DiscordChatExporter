---
title: "docs: Prune PR 1538 stale Latest blocks"
type: docs
status: complete
date: 2026-06-04
origin: /lfg — plan 081 deferred prune stale duplicate Latest blocks from PR body
---

# docs: Prune PR 1538 stale Latest blocks

## Summary

Remove 30+ obsolete `## Latest (...)` sections from PR #1538. Keep Summary, one current `## Latest (plans 070–081)`, Details, updated Validation, CI note, Residual Findings, Post-Deploy, badges. Point maintainers to `docs/recurring-scrape-merge-readiness.md` for full plan history.

## Problem Frame

Plan 080 added a compact Latest block but left legacy per-commit Latest stamps (~250 lines). Reviewers cannot find current operator delta.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | PR body has exactly one `## Latest (plans 070–081 — <HEAD>)` |
| R2 | All other `## Latest (` sections removed |
| R3 | `## Validation Artifacts` and stale checklist removed |
| R4 | Validation section cites `run-all-smokes.sh` 23/23 |
| R5 | Post-Deploy mentions `print-scrape-summary.sh` and cron `--salvage-before-scrape` |
| R6 | `docs/recurring-scrape-merge-readiness.md` plan 082 stamp |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. Merge-readiness

**Files:** `docs/recurring-scrape-merge-readiness.md`

### U2. PR body prune

**Action:** `gh pr edit 1538` with rebuilt body

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
gh pr view 1538 --json body -q '.body' | grep -c '^## Latest ('  # expect 1
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
