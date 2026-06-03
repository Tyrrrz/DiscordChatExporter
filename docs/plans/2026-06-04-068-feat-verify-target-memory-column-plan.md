---
title: "feat: Show per-target container_memory in verify output"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 067 added container_memory to config; verify-operator-ready still only shows global DCE_CONTAINER_MEMORY
---

# feat: Show per-target container_memory in verify output

## Summary

Surface optional `container_memory` in `verify-documents-archives.sh` archive table and print config-driven target memory hints in `verify-operator-ready.sh` when global `DCE_CONTAINER_MEMORY` is unset.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Archive verify table includes MEM column (`container_memory` or `-`) |
| R2 | `verify-operator-ready` lists targets with config memory when global cap unset |
| R3 | Global `DCE_CONTAINER_MEMORY` line unchanged when set in scrape.env |
| R4 | Smokes assert MEM column and target memory hint |
| R5 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. Verify scripts

**Files:** `scripts/verify-documents-archives.sh`, `scripts/verify-operator-ready.sh`, smokes

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Structured JSON run logs
