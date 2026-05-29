---
title: fix: Close recurring scrape residual review findings
type: fix
status: completed
date: 2026-05-29
origin: LFG — residual review findings on feat/recurring-cli-scrape (R1–R3)
---

# fix: Close recurring scrape residual review findings

## Summary

Address manual review residuals from plan 011: correct incremental cursor selection, improve guild channel discovery errors, and document portable archive mount configuration.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `last_message_id` picks highest snowflake reliably across mixed digit lengths |
| R2 | `load_guild_channel_cache` surfaces CLI failure output like `load_guild_cache` |
| R3 | `docker-compose.yml` documents required `DCE_ARCHIVE_ROOT` override |
| R4 | Existing smoke tests pass after changes |

## Implementation Units

### U1. Fix message cursor (`last_message_id`)

**Files:** `scripts/run-discord-scrape.sh`

**Approach:** Replace `max_by(.id)` with `sort_by(.id) | last | .id` for lexicographic ordering on zero-padded-equal-length snowflakes; Discord IDs in one channel are typically same length — sort_by is safer than max_by for strings.

### U2. Guild channel cache diagnostics

**Files:** `scripts/run-discord-scrape.sh`

**Approach:** Capture `channels` CLI stderr/stdout; `die` with context on failure.

### U3. Compose portability note

**Files:** `docker-compose.yml`

**Approach:** Comment above `DCE_ARCHIVE_ROOT` volume line.

## Verification

- `scripts/tests/run-discord-scrape-smoke.sh`
- `bash -n scripts/run-discord-scrape.sh`
