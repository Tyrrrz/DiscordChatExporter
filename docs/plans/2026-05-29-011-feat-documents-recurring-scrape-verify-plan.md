---
title: feat: Documents recurring scrape verification and operator closure
type: feat
status: completed
date: 2026-05-29
origin: LFG — Docker/cron append-only Discord scrape for ~/Documents archive folders
---

# feat: Documents recurring scrape verification and operator closure

## Summary

Close the recurring Discord scrape vertical slice: source-built Docker image, compose mounts for `config/scrape-targets.json` and `/home/brunner56/Documents` archives, append-only JSON merge in `scripts/run-discord-scrape.sh`, monthly cron via `scripts/setup-cron.sh`, and runtime proof (preflight + incremental scrape on at least one enabled target).

## Problem Frame

Operators need monthly (configurable) incremental exports into existing `~/Documents/*_discord*` folders without re-downloading full history or overwriting archives when Discord deletes messages server-side. Infrastructure exists on `feat/recurring-cli-scrape`; this pass validates end-to-end behavior and documents the operator path.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `Dockerfile` builds `DiscordChatExporter.Cli` from source; compose mounts config, scripts, and `archive_root` |
| R2 | `config/scrape-targets.json` maps user Documents folders; empty `channel_ids` exports all accessible channels per target |
| R3 | `run-discord-scrape.sh` uses `--after` + merge-by-id; rejects shrink merges |
| R4 | `setup-cron.sh` defaults to monthly schedule; supports `--target`, `--guild`, `--channel`, `--interval`, `--cron` |
| R5 | `scrape.env` (gitignored) supplies token for compose; never commit secrets |
| R6 | Preflight and one-target scrape succeed against live Discord API |
| R7 | Smoke tests pass; operator docs list validation commands |

## Scope Boundaries

- No changes to upstream C# merge API (wrapper-only append).
- Do not enable `discord_dms` without user token.
- Token stays in `scrape.env` only.

## Implementation Units

### U1. Harden bootstrap and compose paths

**Requirements:** R1, R2

**Files:** `scripts/run-discord-scrape.sh`, `docker-compose.yml`, `Dockerfile`

**Test scenarios:** Archive seed files bootstrap channel-map; compose bind-mount resolves host Documents path.

### U2. Cron installer and docs alignment

**Requirements:** R4, R7

**Files:** `scripts/setup-cron.sh`, `.docs/Recurring-Scrape-Setup.md`, `Readme.md`

**Test scenarios:** `setup-cron.sh --dry-run` emits monthly block; `--remove` idempotent.

### U3. Runtime verification

**Requirements:** R5, R6

**Commands:** `docker compose build`, `run-discord-scrape-host.sh preflight`, scrape `--target` with smallest enabled archive.

**Test scenarios:** Message count non-decreasing after scrape; logs show `--after` when archive non-empty.

## Verification Ladder

1. `bash -n` on changed shell scripts
2. `scripts/tests/setup-cron-smoke.sh`, `run-discord-scrape-smoke.sh`
3. `docker compose build` + preflight + single-target scrape
