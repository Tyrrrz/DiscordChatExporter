---
title: "feat: Channel-filtered prove-incremental-append snapshots"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — prove-incremental-append accepts --channel for scrape but snapshots all archives; yes_general proof should assert only the target channel
---

# feat: Channel-filtered prove-incremental-append snapshots

## Summary

When `prove-incremental-append.sh` is invoked with `--channel`, limit before/after snapshots and grow-only comparison to those channel IDs only.

## Problem

KotOR targets have dozens of channel JSON files. A yes_general-only proof run still snapshots and compares every archive, making failures harder to interpret and unrelated channels part of the pass/fail surface.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `snapshot_archives` skips archives whose channel ID is not in the `--channel` filter when filter is non-empty |
| R2 | Full prove flow applies the same filter to before and after snapshots |
| R3 | `--snapshot-only` honors `--channel` filter |
| R4 | Usage documents channel-scoped snapshot behavior |
| R5 | Smoke asserts filtered snapshot excludes other valid channels |
| R6 | `run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. Filtered snapshots

**Files:** `scripts/prove-incremental-append.sh`, `scripts/tests/prove-incremental-append-smoke.sh`

## Verification

```bash
./scripts/tests/prove-incremental-append-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Per-target memory in config JSON
