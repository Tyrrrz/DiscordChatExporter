---
title: "feat: Salvage flags on operator validation and proof"
type: feat
status: active
date: 2026-06-04
origin: /lfg — plan 054 added documents --salvage-only; operator-validation/proof still lack salvage entry points for yes_general catch-up
---

# feat: Salvage flags on operator validation and proof

## Summary

Add `--salvage-only` and `--salvage-before-scrape` to `run-operator-validation.sh` and `run-operator-proof.sh`, forwarding to `run-documents-scrape.sh --salvage-only` and the normal scrape path.

## Problem Frame

After stopping a long KotOR export, operators need:

```bash
./scripts/run-operator-validation.sh --salvage-only --target KotOR_discord_msgs --channel 221726893064454144
```

Plan 054 implemented salvage on documents-scrape, but validation/proof orchestrators do not expose it.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-operator-validation.sh` accepts `--salvage-only` (salvage + audit, no Discord scrape) |
| R2 | `run-operator-validation.sh` accepts `--salvage-before-scrape` (salvage then scrape + audit) |
| R3 | Both flags forward `--target` / `--channel` to documents scrape |
| R4 | `--salvage-only` and `--salvage-before-scrape` are mutually exclusive |
| R5 | `run-operator-proof.sh` accepts `--salvage-before-scrape` and runs salvage before scrape+prove |
| R6 | Smokes cover validation salvage-only; `run-all-smokes.sh` passes |

## Implementation Units

### U1. Operator validation salvage flags

**Files:** `scripts/run-operator-validation.sh`, `scripts/tests/run-operator-validation-smoke.sh`

### U2. Operator proof salvage-before

**Files:** `scripts/run-operator-proof.sh`

### U3. Smoke gate

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`

## Scope Boundaries

### Deferred

- Killing stale PID 3868255 on host
- Live yes_general catch-up inside LFG
