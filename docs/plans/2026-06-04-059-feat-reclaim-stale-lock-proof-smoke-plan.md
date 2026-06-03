---
title: "feat: Reclaim stale scrape lock and proof salvage-before smoke"
type: feat
status: active
date: 2026-06-04
origin: /lfg — stale MyBook validation leaves lock/meta; proof lacks salvage-before smoke
---

# feat: Reclaim stale scrape lock and proof salvage-before smoke

## Summary

Add `--reclaim-stale` to `scrape-lock-status.sh` for operators to clear dead-holder lock artifacts, and extend `run-operator-proof-smoke.sh` for `--salvage-before-scrape`.

## Problem Frame

After a crashed scrape, `{archive_root}/.dce-scrape.lock.meta` may reference a dead pid. Operators need a safe reclaim path before restarting KotOR catch-up.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scrape-lock-status.sh --reclaim-stale` removes stale `.meta` when holder pid is not running |
| R2 | Reclaim refuses when flock is actively held or holder pid is running |
| R3 | Reclaim removes unheld orphan lock file when safe |
| R4 | `run-operator-proof-smoke.sh` covers `--salvage-before-scrape` |
| R5 | `run-all-smokes.sh` passes |

## Implementation Units

### U1. Lock reclaim flag

**Files:** `scripts/scrape-lock-status.sh`, `scripts/tests/scrape-lock-status-smoke.sh`

### U2. Proof salvage-before smoke

**Files:** `scripts/tests/run-operator-proof-smoke.sh`

## Scope Boundaries

### Deferred

- GUI bridge doc refresh
- Live KotOR catch-up on host
