---
title: "feat: OOM skip labels and operator hints in scrape summary"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 063–064 added DCE_CONTAINER_MEMORY docs; scrape summary still labels OOM skips as generic SKIPPED with no next-step hint
---

# feat: OOM skip labels and operator hints in scrape summary

## Summary

Distinguish OOM/aborted export skips in the scrape run summary and print a one-line operator hint when they occur. Show configured `DCE_CONTAINER_MEMORY` in `verify-operator-ready` output.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `export_channel_incremental` classifies OOM/aborted skips (exit 134/137/139) |
| R2 | Run summary uses `SKIPPED_OOM` with distinct log line vs generic `SKIPPED` |
| R3 | When any `SKIPPED_OOM`, summary footer hints `DCE_CONTAINER_MEMORY` and `--salvage-before-scrape` |
| R4 | `verify-operator-ready.sh` prints non-zero `DCE_CONTAINER_MEMORY` from `scrape.env` |
| R5 | Smokes assert `SKIPPED_OOM` for abort fixture and memory line in verify output |
| R6 | `run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. OOM skip classification and summary hints

**Files:** `scripts/run-discord-scrape.sh`, `scripts/tests/run-discord-scrape-smoke.sh`

### U2. Verify-operator-ready memory line

**Files:** `scripts/verify-operator-ready.sh`, `scripts/tests/verify-operator-ready-smoke.sh`

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
./scripts/tests/verify-operator-ready-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target memory in config JSON
