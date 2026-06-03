---
title: "feat: Documents scrape lock gate and salvage-before"
type: feat
status: active
date: 2026-06-04
origin: /lfg — validation/handoff expose salvage; direct run-documents-scrape.sh still lacks lock gate and --salvage-before-scrape
---

# feat: Documents scrape lock gate and salvage-before

## Summary

Add scrape lock preflight and `--salvage-before-scrape` to `run-documents-scrape.sh` so direct document scrapes match operator-validation safety and KotOR catch-up workflow.

## Problem Frame

Operators often invoke documents scrape directly:

```bash
./scripts/run-documents-scrape.sh --target KotOR_discord_msgs --channel 221726893064454144
```

This bypasses `run-operator-validation.sh` lock gate. Salvage-before requires two commands today.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-documents-scrape.sh` checks archive-root lock before salvage or Discord scrape |
| R2 | Lock gate skipped when `DCE_SKIP_SCRAPE_LOCK=1` |
| R3 | `--salvage-before-scrape` runs salvage then preflight/scrape |
| R4 | `--salvage-only`, `--salvage-before-scrape`, and `--dry-run` are mutually exclusive |
| R5 | Smokes cover lock block and salvage-before; `run-all-smokes.sh` passes |

## Implementation Units

### U1. Documents scrape lock + salvage-before

**Files:** `scripts/run-documents-scrape.sh`

### U2. Smoke coverage

**Files:** `scripts/tests/documents-scrape-smoke.sh`

## Scope Boundaries

### Deferred

- Operator checklist doc refresh
- Live KotOR catch-up on host
