---
title: "fix: Skip active stale temps and retry salvage merge"
type: fix
status: complete
date: 2026-06-04
origin: /lfg — yes_general logs show Stale temp merge failed while export.json still growing (73MB+ invalid JSON)
---

# fix: Skip active stale temps and retry salvage merge

## Problem

`salvage_stale_temp_exports` can run while a channel export is still writing `export.json`. The file is truncated/invalid, `merge_exports_auto` fails, and the temp is retained — but the next incremental pass hits the same race. Observed on KotOR `yes_general` (`221726893064454144`): merge fails on ~82MB partial temp while archive stays at 266182 messages (2021 cursor).

Salvage after export completes works (truncated temp → 79529 messages merges to 345711 in ~58s).

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Skip stale temp dirs whose `export.json` was modified within `DCE_STALE_TEMP_MIN_AGE_SECONDS` (default 120) |
| R2 | On merge failure, re-run `salvage_truncated_json` and retry merge once before retaining temp |
| R3 | Log merge retry vs skip-active with distinct messages |
| R4 | Offline smoke: active temp skipped; retry succeeds after simulated truncation |
| R5 | `run-all-smokes.sh` passes |

## Implementation

- `scripts/run-discord-scrape.sh` — `stale_temp_is_active`, skip guard, merge retry helper
- `scripts/tests/run-discord-scrape-smoke.sh` — active-temp skip + merge-retry scenarios

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- Completing yes_general catch-up inside LFG
- Container memory limits
