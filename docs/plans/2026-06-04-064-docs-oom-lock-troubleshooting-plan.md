---
title: "docs: OOM, scrape lock, and salvage troubleshooting"
type: docs
status: complete
date: 2026-06-04
origin: /lfg — plan 063 added DCE_CONTAINER_MEMORY; operator checklist and GUI bridge cover salvage/lock but Recurring-Scrape-Troubleshooting.md still lacks these runbook sections
---

# docs: OOM, scrape lock, and salvage troubleshooting

## Summary

Extend operator-facing docs so OOM skips, scrape-lock contention, partial `.dce-temp` salvage, and `DCE_CONTAINER_MEMORY` are documented in the troubleshooting guide and GUI bridge quick-start.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `.docs/Recurring-Scrape-Troubleshooting.md` documents OOM/skipped channels and `DCE_CONTAINER_MEMORY` |
| R2 | Same file documents scrape lock held, twin runs, and `--reclaim-stale` |
| R3 | Same file documents partial `.dce-temp`, `--salvage-only`, and `--salvage-before-scrape` |
| R4 | `docs/gui-zip-recurring-scrape-bridge.md` mentions `DCE_CONTAINER_MEMORY=8g` for yes_general catch-up |
| R5 | `.docs/Recurring-Scrape-Setup.md` links or notes container memory for large channels |
| R6 | `sync-gui-bridge-doc-smoke.sh` still passes; `run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. Troubleshooting runbook sections

**Files:** `.docs/Recurring-Scrape-Troubleshooting.md`

Add under Export Issues (or new Runtime section):
- Channel SKIPPED / OOM / Aborted
- Scrape lock already held
- Stale partial exports under `.dce-temp`

### U2. Setup and GUI bridge cross-links

**Files:** `.docs/Recurring-Scrape-Setup.md`, `docs/gui-zip-recurring-scrape-bridge.md`

- Setup: disk/memory note + pointer to troubleshooting
- GUI bridge: `DCE_CONTAINER_MEMORY` in yes_general salvage block

## Verification

```bash
./scripts/tests/sync-gui-bridge-doc-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target memory in `scrape-targets.json`
