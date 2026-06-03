---
title: "fix: Salvage stale temp exports before re-downloading"
type: fix
status: active
date: 2026-06-03
origin: /lfg — yes_general re-downloads 514 MB because prior aborted run's temp data is never recovered
---

# fix: Salvage stale temp exports before re-downloading

## Problem

`scrape_target()` always creates a fresh temp directory and starts `export_channel_incremental` from `last_message_id(archive)`. When a previous run crashes (OOM, abort, kill), the partially-downloaded temp export is orphaned under `.dce-temp/export.<channel_id>.*` but never cleaned up or reused.

For `yes_general` (channel `221726893064454144`):
- Archive last message: **2021-01-17** (`800354246440648745`), 264K messages, 312 MB
- Stale temp export from May 29: **514 MB** of truncated JSON (messages 2021→mid-2026)
- Every re-run downloads all of those messages again from scratch

The `salvage-truncated-export.sh` script already exists but is never called automatically.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Before exporting a channel, `scrape_target` checks for orphaned temp dirs matching `.dce-temp/export.<channel_id>.*` |
| R2 | If an orphaned temp export contains truncated JSON, salvage it to valid JSON using the same logic as `salvage-truncated-export.sh` |
| R3 | If salvage succeeds, merge the recovered messages into the archive (same merge_exports + commit_merged_export path) |
| R4 | Clean up stale temp dirs after salvage (success or failure) |
| R5 | After salvage-merge, `last_message_id` returns the advanced ID so the incremental only fetches truly new messages |
| R6 | If salvage fails (can't find a safe truncation point), delete the stale temp and proceed normally with a full incremental |
| R7 | Existing 19 smokes + new salvage smoke pass |

## Files

- `scripts/run-discord-scrape.sh` — add `salvage_stale_temp_exports()` called at top of per-channel loop in `scrape_target()`
- `scripts/tests/run-discord-scrape-smoke.sh` — add `salvage-stale` smoke: seed a truncated temp export, run scrape, verify messages are merged and `--after` advances

## Implementation

### `salvage_stale_temp_exports()`

```
salvage_stale_temp_exports(output_dir, channel_id):
  glob = output_dir/.dce-temp/export.<channel_id>.*/export.json
  for each stale_export matching glob:
    if jq empty succeeds → already valid JSON
    else → run inline python salvage (same as salvage-truncated-export.sh)
    if salvage fails → rm -rf stale_dir, continue
    
    validate channel identity
    if archive exists:
      merge_exports(archive, stale_export, temp_merged)
      commit_merged_export(archive, temp_merged)
      log "SALVAGED" with message counts
    else:
      mv stale_export → archive destination
    rm -rf stale_dir
```

Called in `scrape_target()` before line 963 (`after_id=$(last_message_id ...)`), so the salvaged data is already in the archive when `--after` is computed.

### Smoke test

1. Seed archive with 2 messages (existing fixture)
2. Create a fake stale `.dce-temp/export.<channel_id>.STALE/export.json` with truncated JSON containing message id "3"
3. Run scrape in append mode
4. Verify archive has 3+ messages (salvaged + incremental)
5. Verify stale temp dir is cleaned up

## Test scenarios

| Scenario | Expected |
|----------|----------|
| Stale temp with truncated JSON → salvageable | Messages merged, temp cleaned, `--after` advances |
| Stale temp with unsalvageable data (too short) | Temp deleted, normal incremental proceeds |
| Stale temp with valid JSON (complete export) | Merged directly, temp cleaned |
| No stale temps | Normal behavior, no change |
| Multiple stale temps for same channel | All salvaged in order, then normal incremental |

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- Configurable skip-vs-retry for OOM channels (separate concern)
- Increasing container memory limits
