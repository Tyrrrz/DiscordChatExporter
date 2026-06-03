---
title: "fix: Treat SIGTERM/SIGINT export aborts as skippable"
type: fix
status: complete
date: 2026-06-04
origin: /lfg — yes_general died with fatal ERROR instead of SKIPPED when validation was stopped
---

# fix: Treat SIGTERM/SIGINT export aborts as skippable

## Problem

Killing overlapping validation (`kill` / Ctrl+C) sends SIGTERM/SIGINT to the export CLI. Bash exit codes **143** (128+15) and **130** (128+2) were not in the abort-skip list, so `scrape_target` called `die` and failed the whole target — losing partial temp progress instead of SKIPPED + preserve.

Observed: `ERROR: Channel 221726893064454144 failed` at 7% progress in `logs/kotor-validation-20260604.log`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `export_channel_incremental` returns skip (2) on exit 130 and 143, same as 134/137/139 |
| R2 | Partial temp preserved on those skips when `temp_export` non-empty |
| R3 | Offline smoke covers exit 143 skip path |
| R4 | `.gitignore` includes `.dce-scrape.lock` |
| R5 | `run-all-smokes.sh` passes; docs updated |

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- Completing yes_general catch-up inside LFG
- Container memory tuning
