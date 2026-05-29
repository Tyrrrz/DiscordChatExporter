---
title: fix: Salvage corrupt archive JSON and harden scrape loop
type: fix
status: complete
date: 2026-05-29
origin: LFG — KotOR yes_general export truncated; prove/cron fail on jq parse
---

# fix: Salvage corrupt archive JSON and harden scrape loop

## Summary

One KotOR archive JSON is truncated mid-message. Add audit/salvage tooling and make prove/scrape skip or repair invalid files without aborting entire targets.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `audit-archive-json.sh` lists invalid JSON per target/output_dir |
| R2 | `salvage-truncated-export.sh` backs up and repairs truncated DCE exports |
| R3 | `prove-incremental-append.sh` skips invalid JSON with warning (not fatal) |
| R4 | Salvaged KotOR file passes `jq empty` and prove for that target |
| R5 | Smoke test for audit script |

## Implementation Units

### U1. Audit + salvage scripts

**Files:** `scripts/audit-archive-json.sh`, `scripts/salvage-truncated-export.sh`

### U2. Prove hardening

**Files:** `scripts/prove-incremental-append.sh`

### U3. Repair KotOR file (runtime)

**File:** `~/Documents/KotOR_discord_msgs/...yes_general [221726893064454144].json`

## Verification

- `jq empty` on salvaged file
- `prove-incremental-append.sh --target KotOR_discord_msgs`
