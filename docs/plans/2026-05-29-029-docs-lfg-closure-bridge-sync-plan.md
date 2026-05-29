---
title: docs: LFG closure — sync operator docs and GUI bridge
type: docs
status: complete
date: 2026-05-29
origin: /lfg — feature stack complete; align GUI zip bridge and README with operator-handoff
---

# docs: LFG closure — sync operator docs and GUI bridge

## Summary

Recurring scrape implementation is complete. Update operator-facing docs so GUI zip users and README readers see `operator-handoff.sh`, disk preflight, and the correct `run-documents-scrape.sh` invocation (no `scrape` subcommand).

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `Readme.md` See also mentions `operator-handoff.sh` |
| R2 | `.docs/Recurring-Scrape-Setup.md` quick path includes handoff + disk note |
| R3 | `DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` updated (sibling bridge) |
| R4 | `run-all-smokes.sh` still passes (17 smokes) |

## Verification

- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
