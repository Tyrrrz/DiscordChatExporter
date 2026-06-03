---
title: "docs: Salvage and lock operator playbook"
type: docs
status: active
date: 2026-06-04
origin: /lfg — plans 054–059 landed salvage/lock tooling; operator docs still show 19 smokes and omit catch-up playbook
---

# docs: Salvage and lock operator playbook

## Summary

Refresh operator-facing docs with scrape lock diagnostics, salvage flags, and KotOR yes_general catch-up playbook. Sync GUI bridge copy.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `gui-zip-recurring-scrape-bridge.md` documents lock status, reclaim, salvage-only, salvage-before |
| R2 | `recurring-scrape-operator-checklist.md` adds stuck-channel / partial temp section |
| R3 | `recurring-scrape-merge-readiness.md` reflects 21 smokes and plans 054–059 |
| R4 | Run `sync-gui-bridge-doc.sh` when sibling GUI zip path exists |

## Implementation Units

### U1. Operator doc updates

**Files:** `docs/gui-zip-recurring-scrape-bridge.md`, `docs/recurring-scrape-operator-checklist.md`, `docs/recurring-scrape-merge-readiness.md`

### U2. GUI bridge sync

**Command:** `./scripts/sync-gui-bridge-doc.sh`

## Scope Boundaries

### Deferred

- Live KotOR catch-up execution on host
- `.docs/Recurring-Scrape-Setup.md` full rewrite
