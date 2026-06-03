---
title: "feat: Per-target container_memory in scrape-targets.json"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 063 deferred per-target memory; KotOR yes_general catch-up should auto-apply 8g when scraping KotOR_discord_msgs alone without global scrape.env change
---

# feat: Per-target container_memory in scrape-targets.json

## Summary

Optional `container_memory` on each target in `scrape-targets.json`. When `run-discord-scrape-host.sh` runs with exactly one `--target`, apply that target's memory cap to compose unless `DCE_CONTAINER_MEMORY` is already set non-zero in the environment or `scrape.env`.

## Problem

Operators must set global `DCE_CONTAINER_MEMORY=8g` in `scrape.env` for KotOR `yes_general` catch-up, affecting every scrape including lightweight targets. A per-target knob keeps default runs unlimited while auto-raising memory for KotOR-only runs.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Targets may include optional `container_memory` (e.g. `8g`) |
| R2 | Host runner applies it when exactly one `--target` is selected and global `DCE_CONTAINER_MEMORY` is unset or `0` |
| R3 | Explicit `DCE_CONTAINER_MEMORY` in shell or `scrape.env` wins over config |
| R4 | Run plan banner shows per-target memory when applied |
| R5 | `KotOR_discord_msgs` configured with `container_memory: "8g"` |
| R6 | Host smoke asserts config-driven memory when env omits global cap |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. Config field + host apply

**Files:** `scripts/lib/scrape-run-plan.sh`, `scripts/run-discord-scrape-host.sh`, `config/scrape-targets.json`

### U2. Smoke + docs

**Files:** `scripts/tests/run-discord-scrape-host-smoke.sh`, `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`

## Verification

```bash
./scripts/tests/run-discord-scrape-host-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up execution inside LFG
- Numeric max across multiple `--target` flags
- Structured JSON run logs
