---
title: "feat: Salvage-only mode for stale temp exports"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — yes_general has 154MB+ active partial temp while full-target validation runs; operator needs to merge stale temps without re-downloading years of history after stopping a run
---

# feat: Salvage-only mode for stale temp exports

## Summary

Add a `salvage` subcommand that merges quiescent `.dce-temp` exports into archives without calling Discord, wired through the host runner and `run-documents-scrape.sh --salvage-only`.

## Problem Frame

After stopping a long-running or OOM-aborted export, operators must advance the archive cursor from preserved partial temps. Today salvage only runs at the start of a full `scrape`, which re-fetches from the archive `--after` cursor and can repeat multi-year catch-up on `yes_general`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-discord-scrape.sh salvage` merges stale temps per resolved channel; no Discord CLI export calls |
| R2 | Salvage mode does not require `DISCORD_TOKEN` |
| R3 | Existing `DCE_SALVAGE_ACTIVE_TEMPS=1` and `DCE_STALE_TEMP_MIN_AGE_SECONDS` env behavior applies |
| R4 | `run-discord-scrape-host.sh salvage` acquires archive-root lock and runs salvage locally (no compose/token) |
| R5 | `run-documents-scrape.sh --salvage-only` skips preflight/scrape and invokes host salvage |
| R6 | Smoke covers salvage subcommand; `run-all-smokes.sh` passes |

## Key Technical Decisions

- **Local host execution for salvage**: Avoids compose/token requirements; salvage is filesystem-only.
- **Reuse `salvage_stale_temp_exports`**: Same merge path as scrape preamble; no duplicate logic.

## Implementation Units

### U1. Core salvage subcommand

**Goal:** `salvage_only_target` + `run_target_mode salvage` without token gate.

**Requirements:** R1–R3

**Files:** `scripts/run-discord-scrape.sh`, `scripts/tests/run-discord-scrape-smoke.sh`

### U2. Host and documents wiring

**Goal:** Operator entry points for salvage-only.

**Requirements:** R4–R5

**Files:** `scripts/run-discord-scrape-host.sh`, `scripts/run-documents-scrape.sh`, `scripts/tests/documents-scrape-smoke.sh`

### U3. Smoke gate

**Requirements:** R6

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`

## Scope Boundaries

### Deferred

- Killing stale validation on host
- Live yes_general catch-up inside LFG
- Container memory tuning
