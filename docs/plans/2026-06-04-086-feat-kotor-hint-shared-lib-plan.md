---
title: "feat: shared KotOR catch-up hint lib"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 085 added handoff hint; verify-operator-ready and bootstrap still omit KotOR wrapper
---

# feat: shared KotOR catch-up hint lib

## Summary

Extract `scripts/lib/kotor-catchup-hint.sh` from operator-handoff and call it from `verify-operator-ready.sh` and `bootstrap-recurring-scrape.sh` when `KotOR_discord_msgs` is enabled.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Shared lib prints wrapper + summary inspect lines |
| R2 | `operator-handoff.sh` sources lib (no duplicated jq logic) |
| R3 | `verify-operator-ready.sh` prints hint after "Operator ready. Next:" |
| R4 | `bootstrap-recurring-scrape.sh` prints hint after dry-run and live bootstrap complete |
| R5 | `scrape.env.example` cites `run-kotor-yes-general-catchup.sh` for yes_general |
| R6 | Smokes assert hint from verify + bootstrap KotOR fixtures |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 24/24 |

## Implementation Units

### U1. Lib + script wiring

**Files:** `scripts/lib/kotor-catchup-hint.sh`, `scripts/operator-handoff.sh`, `scripts/verify-operator-ready.sh`, `scripts/bootstrap-recurring-scrape.sh`, `scrape.env.example`

### U2. Smokes + docs

**Files:** `scripts/tests/verify-operator-ready-smoke.sh`, `scripts/tests/bootstrap-recurring-scrape-smoke.sh`, `docs/recurring-scrape-merge-readiness.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on operator host
