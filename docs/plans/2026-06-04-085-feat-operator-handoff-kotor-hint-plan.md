---
title: "feat: operator-handoff KotOR catch-up hint"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 084 synced docs; handoff still omits KotOR wrapper from post-complete next steps
---

# feat: operator-handoff KotOR catch-up hint

## Summary

When `KotOR_discord_msgs` is enabled in scrape config, `operator-handoff.sh` prints a post-complete hint pointing at `run-kotor-yes-general-catchup.sh`. Refresh merge-readiness HEAD and PR #1538 Latest for plans 070–085.

## Problem Frame

Operators run `operator-handoff.sh` before first scrape or cron install. After plan 083, the KotOR yes_general path lives in a dedicated wrapper, but handoff only suggests generic `run-documents-scrape.sh`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | After successful handoff (dry-run or salvage-only), if `KotOR_discord_msgs` is enabled, print KotOR wrapper hint |
| R2 | Hint includes `run-kotor-yes-general-catchup.sh` and summary inspect line |
| R3 | No hint when KotOR target disabled or absent |
| R4 | `operator-handoff-smoke.sh` covers hint with KotOR fixture config |
| R5 | Merge-readiness HEAD + plan 085 bullet |
| R6 | PR #1538 Latest → plans 070–085 |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 24/24 |

## Implementation Units

### U1. Handoff hint + smoke

**Files:** `scripts/operator-handoff.sh`, `scripts/tests/operator-handoff-smoke.sh`

### U2. Docs + PR

**Files:** `docs/recurring-scrape-merge-readiness.md`, PR #1538 body via `gh pr edit`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on operator host
- Auto-sync GUI zip `RECURRING-SCRAPE.md` (operator runs `sync-gui-bridge-doc.sh`)
