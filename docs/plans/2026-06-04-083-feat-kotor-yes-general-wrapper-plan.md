---
title: "feat: KotOR yes_general catch-up wrapper"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 082 deferred live KotOR catch-up; encode operator path as one script
---

# feat: KotOR yes_general catch-up wrapper

## Summary

Add `scripts/run-kotor-yes-general-catchup.sh` — thin wrapper for channel `221726893064454144` with default log/summary paths, `--salvage-before-scrape`, and subcommands for validation/prove/salvage-only/dry-run.

## Problem Frame

KotOR yes_general catch-up is documented in five places with long flag chains. Operators need one entry point; LFG still cannot run live Discord scrape in CI.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Default live run: `--salvage-before-scrape` + documents scrape + `--log-file logs/kotor-yes-general.log` |
| R2 | `--dry-run`, `--salvage-only`, `--validation`, `--prove` modes |
| R3 | `--log-file` and `--config` overrides |
| R4 | Prints summary inspect hint after live scrape |
| R5 | `kotor-yes-general-catchup-smoke.sh` dry-run passes offline |
| R6 | Docs updated; smoke count **24/24** in setup doc + merge-readiness |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 24/24 |

## Implementation Units

### U1. Wrapper script

**Files:** `scripts/run-kotor-yes-general-catchup.sh`, `scripts/tests/kotor-yes-general-catchup-smoke.sh`

### U2. Docs

**Files:** `docs/recurring-scrape-merge-readiness.md`, `docs/recurring-scrape-operator-checklist.md`, `.docs/Recurring-Scrape-Setup.md`

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Running live KotOR catch-up inside LFG/CI (operator host only)
