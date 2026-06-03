---
title: "feat: Cron uses documents scrape with --log-file"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 078 deferred wire --log-file into setup-cron crontab line
---

# feat: Cron uses documents scrape with --log-file

## Summary

Change `setup-cron.sh` to install `run-documents-scrape.sh --log-file PATH` instead of `run-discord-scrape-host.sh scrape >> log`. Cron jobs get archive verify, disk preflight, lock gate, teed logs, and paired JSON summaries.

## Problem Frame

Plan 078 added `--log-file` tee to documents scrape, but monthly cron still invokes the bare host wrapper with shell `>>` redirect — bypassing the unified workflow and JSON summary pairing.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Cron job line runs `run-documents-scrape.sh --config HOST_CONFIG --log-file LOG_FILE` |
| R2 | `--target`, `--channel`, `--guild` forwarded to documents scrape |
| R3 | Cron sets `DCE_ENV_FILE`, `DCE_COMPOSE_FILE`, `DCE_COMPOSE_TTY=0` (no shell `>>` redirect) |
| R4 | `run-documents-scrape.sh` accepts `--guild` passthrough like `--channel` |
| R5 | `setup-cron-smoke.sh` asserts documents scrape + `--log-file` in crontab |
| R6 | Docs note cron log + `<basename>.summary.json` |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 23/23 |

## Implementation Units

### U1. setup-cron.sh

**Files:** `scripts/setup-cron.sh`, `scripts/tests/setup-cron-smoke.sh`

### U2. run-documents-scrape.sh

**Files:** `scripts/run-documents-scrape.sh` (--guild passthrough)

### U3. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `.docs/Recurring-Scrape-Setup.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Refresh PR #1538 body with plans 070–079 stamps
- `--salvage-before-scrape` on cron (operator opt-in only)
