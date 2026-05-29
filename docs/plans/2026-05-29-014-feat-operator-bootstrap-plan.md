---
title: feat: One-command recurring scrape bootstrap
type: feat
status: completed
date: 2026-05-29
origin: LFG — close operator gap between GUI zip workspace and first successful scrape/cron
---

# feat: One-command recurring scrape bootstrap

## Summary

Provide `bootstrap-recurring-scrape.sh` so operators run verify → docker build → preflight in one command, plus smoke coverage and GUI-workspace stubs.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `bootstrap-recurring-scrape.sh` supports `--dry-run`, `--skip-build`, `--target` |
| R2 | `scrape-here.sh` forwards `--help` to container scrape help |
| R3 | `scrape-here-smoke.sh` in CI recurring-scrape-smoke job |
| R4 | GUI zip folder has executable bootstrap stub |
| R5 | All existing smoke tests still pass |

## Implementation Units

### U1. Bootstrap script

**Files:** `scripts/bootstrap-recurring-scrape.sh`

### U2. Launcher + smoke

**Files:** `scripts/scrape-here.sh`, `scripts/tests/scrape-here-smoke.sh`, `.github/workflows/main.yml`

### U3. GUI workspace stub

**Files:** `../DiscordChatExporter.linux-x64/bootstrap-recurring-scrape.sh`

## Verification

- `./scripts/bootstrap-recurring-scrape.sh --dry-run`
- `./scripts/tests/scrape-here-smoke.sh`
