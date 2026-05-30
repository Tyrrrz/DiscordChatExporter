---
title: "feat: Scrape logging, run summary, and default-all-targets"
type: feat
status: complete
date: 2026-05-29
origin: /lfg — operator scripts need explicit server/file/message visibility and sane defaults
---

# feat: Scrape logging, run summary, and default-all-targets

## Summary

Make recurring scrape scripts print a upfront run plan (which guilds/servers, which output folders), per-channel file I/O with message deltas, and a final change summary. Operator entrypoints default to all enabled targets from `config/scrape-targets.json` without requiring repeated `--target` flags.

## Problem Frame

Operators cannot tell from current logs which Discord server was scraped, which archive files were touched, or how many messages were appended vs unchanged. `run-operator-proof.sh` still hardcodes `eod_discord`. The core engine (`run-discord-scrape.sh`) logs channel IDs but not guild names, paths, or before/after counts in one place.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Before scrape/preflight, log config path and every selected target with `name`, resolved guild id/name(s), and `output_dir` |
| R2 | For each channel processed, log destination file path, action (`CREATED`, `MERGED`, `UNCHANGED`, `SKIPPED`), and message counts before → after (plus fetched batch size when merged) |
| R3 | After all targets complete, print a consolidated run summary with per-file deltas and totals |
| R4 | `run-documents-scrape.sh` and host wrapper print the same run-plan header before invoking the container |
| R5 | `run-operator-proof.sh` defaults to all enabled targets (loop handoff → scrape → prove) when `--target` is omitted |
| R6 | Offline smokes pass; scrape smoke asserts summary markers exist |

## Key Technical Decisions

- **KTD1: Ledger in `run-discord-scrape.sh`:** Keep summary state in bash arrays inside the core script rather than a new shared library — host wrappers only need jq-based target listing; the container owns channel-level detail.
- **KTD2: Guild labels from cache + export metadata:** Resolve guild names from `load_guild_cache` at target start; enrich per-channel lines from export JSON when available.
- **KTD3: No behavior change to merge semantics:** Logging only; append-only merge and skip rules stay unchanged.

## Implementation Units

### U1. Core scrape ledger and summary

**Goal:** Operator-visible run plan, per-channel I/O lines, and final summary in `run-discord-scrape.sh`.

**Requirements:** R1, R2, R3

**Files:** `scripts/run-discord-scrape.sh`, `scripts/tests/run-discord-scrape-smoke.sh`

**Approach:** Add `SCRAPE_SUMMARY_ENTRIES`, `guild_name_for_id`, `describe_target_resolution`, `log_run_plan`, `record_channel_result`, `print_scrape_summary`. Call from `run_target_mode` and `scrape_target`. Preflight reuses run plan header.

**Test scenarios:**
- Happy path: smoke run shows `Scrape run plan`, `MERGED`/`CREATED`/`UNCHANGED` lines, and `Scrape run summary`
- Edge: skipped channel appears as `SKIPPED` in summary
- Error path: failure before summary still leaves partial ledger in stderr

**Verification:** `./scripts/tests/run-discord-scrape-smoke.sh` passes with grep for summary markers.

### U2. Host and documents wrapper banners

**Goal:** Host-side run plan before container execution.

**Requirements:** R4

**Files:** `scripts/run-discord-scrape-host.sh`, `scripts/run-documents-scrape.sh`, `scripts/operator-handoff.sh`

**Approach:** Shared helper pattern: jq list enabled/selected targets with output_dir; print subcommand and config paths. `operator-handoff` lists enabled targets in handoff header.

**Test scenarios:**
- Happy path: documents-scrape dry-run output includes target list
- Integration: host smoke unchanged (no regression)

**Verification:** `./scripts/tests/documents-scrape-smoke.sh`, `./scripts/tests/run-discord-scrape-host-smoke.sh`

### U3. Operator proof defaults to all enabled targets

**Goal:** Remove hardcoded `eod_discord`; loop all enabled targets when `--target` omitted.

**Requirements:** R5

**Files:** `scripts/run-operator-proof.sh`, `scripts/tests/run-operator-proof-smoke.sh` (if present)

**Approach:** When `TARGET` empty, `mapfile` enabled names from config and run handoff once then scrape+prove per target; print per-target summary at end.

**Test scenarios:**
- Happy path: smoke with fake scripts verifies multi-target loop
- Edge: single `--target` still runs one target only

**Verification:** operator-proof smoke or documents smoke + manual grep.

## Scope Boundaries

### Deferred to Follow-Up Work

- Structured JSON run logs for machine parsing
- Changing `prove-incremental-append.sh` to require optional `--target`

### Out of scope

- Discord API or merge algorithm changes
- New CLI flags beyond existing `--target` narrowing
