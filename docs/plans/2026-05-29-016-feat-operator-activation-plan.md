---
title: feat: Operator activation — cron install and bot-token warning
type: feat
status: completed
date: 2026-05-29
origin: LFG — original request included monthly cron; cron not yet installed; bot token blocks live fetch
---

# feat: Operator activation — cron install and bot-token warning

## Summary

Complete the operator slice: warn when preflight only succeeds via seeded archives (typical bot token), install the monthly cron job, and document activation in the checklist.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `bootstrap-recurring-scrape.sh` prints clear bot-token / user-token guidance after seeded-only preflight |
| R2 | Monthly cron block installed via `setup-cron.sh` |
| R3 | Operator checklist notes cron installed and token type |
| R4 | Bootstrap smoke still passes |

## Implementation Units

### U1. Bot-token advisory

**Files:** `scripts/bootstrap-recurring-scrape.sh`

### U2. Install cron (runtime)

**Command:** `./scripts/setup-cron.sh --skip-preflight`

### U3. Docs

**Files:** `docs/recurring-scrape-operator-checklist.md`

## Verification

- `crontab -l` contains `BEGIN discord-scrape`
- `bootstrap-recurring-scrape-smoke.sh`
