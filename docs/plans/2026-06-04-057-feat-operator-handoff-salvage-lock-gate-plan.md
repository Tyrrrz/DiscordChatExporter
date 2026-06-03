---
title: "feat: Operator handoff salvage-only and scrape lock gate"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 056 deferred operator-handoff --salvage-only; proof lacks salvage-only mode and salvage smoke
---

# feat: Operator handoff salvage-only and scrape lock gate

## Summary

Add `--salvage-only` to `operator-handoff.sh` and `run-operator-proof.sh`, fail fast in `run-operator-validation.sh` when the archive-root scrape lock is held, and extend smokes.

## Problem Frame

After stopping a crashed KotOR export, operators need a handoff entry that merges partial temps without dry-run or Discord:

```bash
./scripts/operator-handoff.sh --salvage-only --target KotOR_discord_msgs --channel 221726893064454144
```

Validation should refuse to start a scrape while another checkout holds `{archive_root}/.dce-scrape.lock`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `operator-handoff.sh` accepts `--salvage-only` (runs documents salvage instead of dry-run) |
| R2 | `run-operator-proof.sh` accepts `--salvage-only` (handoff + salvage per target, no scrape/prove) |
| R3 | `run-operator-validation.sh` exits before scrape when lock is actively held |
| R4 | Lock gate skipped when `DCE_SKIP_SCRAPE_LOCK=1` or `--skip-scrape` |
| R5 | Smokes cover handoff and proof salvage-only; `run-all-smokes.sh` passes |

## Implementation Units

### U1. Handoff and proof salvage-only

**Files:** `scripts/operator-handoff.sh`, `scripts/run-operator-proof.sh`, smokes

### U2. Validation lock gate

**Files:** `scripts/run-operator-validation.sh`

### U3. Smoke gate

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Docs refresh for new flags
