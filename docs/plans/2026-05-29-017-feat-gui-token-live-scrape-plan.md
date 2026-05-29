---
title: feat: GUI token sync and live append proof
type: feat
status: completed
date: 2026-05-29
origin: LFG — Settings.dat holds user token; prove incremental scrape works beyond bot-token 403 skips
---

# feat: GUI token sync and live append proof

## Summary

Sync `scrape.env` from DiscordChatExporter GUI `Settings.dat`, add `sync-token-from-gui.sh`, wire bootstrap to suggest it, and verify one live append-only scrape with grow-only proof.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `sync-token-from-gui.sh` writes `scrape.env` from discovered GUI token |
| R2 | Bootstrap mentions GUI sync when forbidden channels detected |
| R3 | Live preflight succeeds with GUI token on at least one target |
| R4 | `prove-incremental-append.sh` passes on one Documents target |
| R5 | linux-x64 `docker-compose` wrapper delegates to source repo |

## Implementation Units

### U1. GUI token sync script

**Files:** `scripts/sync-token-from-gui.sh`

### U2. Bootstrap + linux-x64 wrapper

**Files:** `scripts/bootstrap-recurring-scrape.sh`, `DiscordChatExporter.linux-x64/docker-compose.sh`

### U3. Live verification (runtime)

**Commands:** sync token, scrape one target, prove append
