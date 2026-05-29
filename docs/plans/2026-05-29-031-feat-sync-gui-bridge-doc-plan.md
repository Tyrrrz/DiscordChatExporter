---
title: feat: Sync GUI zip bridge doc from source repo
type: feat
status: complete
date: 2026-05-29
origin: /lfg — plan 013/019 planned sync-workspace-bridge.sh; bridge doc now versioned in git
---

# feat: Sync GUI zip bridge doc from source repo

## Summary

Add `scripts/sync-gui-bridge-doc.sh` to copy `docs/gui-zip-recurring-scrape-bridge.md` into the sibling GUI zip folder as `RECURRING-SCRAPE.md`, with a smoke test using a temp destination.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `sync-gui-bridge-doc.sh` copies bridge doc to configurable dest (default `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md`) |
| R2 | `--dry-run` prints source and destination |
| R3 | `sync-gui-bridge-doc-smoke.sh` verifies copy into temp dir |
| R4 | Operator checklist and gui bridge doc mention the sync script |
| R5 | `run-all-smokes.sh` passes |

## Verification

- `./scripts/tests/sync-gui-bridge-doc-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
