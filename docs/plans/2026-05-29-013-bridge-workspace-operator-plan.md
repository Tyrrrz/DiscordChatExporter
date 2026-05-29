---
title: feat: Bridge GUI workspace to recurring scrape automation
type: feat
status: completed
date: 2026-05-29
origin: LFG — Cursor workspace is DiscordChatExporter.linux-x64; automation lives in source repo
---

# feat: Bridge GUI workspace to recurring scrape automation

## Summary

Operators who open the prebuilt `DiscordChatExporter.linux-x64` folder need a clear path to Docker/cron/append scrape tooling in the sibling source repository without hunting paths.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Source repo documents linux-x64 → source repo handoff |
| R2 | `scripts/scrape-here.sh` dispatches to host runner from any cwd inside repo |
| R3 | `RECURRING-SCRAPE.md` in linux-x64 workspace points to source repo commands |
| R4 | All `scripts/tests/*.sh` pass |

## Implementation Units

### U1. Repo launcher script

**Files:** `scripts/scrape-here.sh`, `Readme.md`

### U2. GUI workspace pointer doc

**Files:** `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` (outside git; operator artifact)

## Verification

- Run all `scripts/tests/*.sh`
- `scripts/scrape-here.sh help` exits 0
