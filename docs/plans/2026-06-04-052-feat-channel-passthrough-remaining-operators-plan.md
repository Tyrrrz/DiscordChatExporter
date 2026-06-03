---
title: "feat: Pass --channel through remaining operator wrappers"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 049 wired --channel on documents-scrape and operator-validation; handoff, prove, and operator-proof still reject it, blocking focused yes_general catch-up
deepened: 2026-06-04
---

# feat: Pass --channel through remaining operator wrappers

## Summary

Complete the operator CLI chain by accepting repeatable `--channel ID` in `operator-handoff.sh`, `prove-incremental-append.sh`, and `run-operator-proof.sh`, forwarding to downstream scripts the same way plan 049 did for documents-scrape and operator-validation.

## Problem Frame

Operators re-running KotOR `yes_general` (`221726893064454144`) need single-channel workflows end-to-end:

```bash
./scripts/run-operator-proof.sh --target KotOR_discord_msgs --channel 221726893064454144
```

Plan 049 fixed `run-documents-scrape.sh` and `run-operator-validation.sh`, but `operator-handoff` dry-run, `prove-incremental-append` scrape step, and `run-operator-proof` orchestration still die on `--channel`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `operator-handoff.sh` accepts repeatable `--channel ID` and forwards to `run-documents-scrape.sh --dry-run` |
| R2 | `prove-incremental-append.sh` accepts repeatable `--channel ID` and forwards to `run-discord-scrape-host.sh scrape` |
| R3 | `run-operator-proof.sh` accepts repeatable `--channel ID` and forwards to handoff, documents scrape, and prove |
| R4 | Usage text on all three scripts documents `--channel` requires exactly one `--target` (enforced downstream) |
| R5 | Smokes cover `--channel` acceptance on handoff dry-run path; prove smoke documents passthrough via arg capture or dry-run stub where feasible |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes |

## Key Technical Decisions

- **Mirror plan 049 passthrough pattern**: collect `--channel` into an array during option parsing; append to downstream invocations unchanged.
- **Snapshot scope unchanged**: `prove-incremental-append` still snapshots all archives under the target; single-channel scrape must not shrink any existing archive (existing compare logic).
- **Handoff only dry-runs scrape**: `--channel` affects the dry-run invocation only; verify-operator-ready remains target-wide.

## Implementation Units

### U1. operator-handoff --channel passthrough

**Goal:** Handoff dry-run accepts and forwards `--channel`.

**Requirements:** R1, R4

**Files:**
- `scripts/operator-handoff.sh`
- `scripts/tests/operator-handoff-smoke.sh`

**Approach:** Parse `--channel` into `CHANNEL_ARGS`; pass `"${CHANNEL_ARGS[@]}"` to `"$DOCUMENTS_SCRAPE" --dry-run`.

**Test scenarios:**
- Handoff with `--target demo --channel 111...` on temp config completes with "Handoff complete" (dry-run path; no live Discord).

**Verification:** `operator-handoff-smoke.sh` passes.

### U2. prove-incremental-append --channel passthrough

**Goal:** Prove script forwards `--channel` to host scrape runner.

**Requirements:** R2, R4

**Files:**
- `scripts/prove-incremental-append.sh`
- `scripts/tests/prove-incremental-append-smoke.sh` (optional arg-capture if host stubbed; at minimum usage/help acceptance)

**Approach:** Parse `--channel` into array; append to `"$HOST_RUNNER" scrape ...` invocation.

**Test scenarios:**
- `--help` or dry-run parse path accepts `--channel` without "Unknown option".
- When fake host runner captures argv, `--channel` appears on scrape command (if smoke infrastructure allows; otherwise extend handoff-style config-only test).

**Verification:** `prove-incremental-append-smoke.sh` passes.

### U3. run-operator-proof --channel passthrough

**Goal:** End-to-end operator proof orchestrator forwards `--channel` to all three steps.

**Requirements:** R3, R4

**Files:**
- `scripts/run-operator-proof.sh`

**Approach:** Parse `--channel` once; pass to `"$HANDOFF"`, `"$DOCUMENTS"`, and `"$PROVE"` when `--target` is set.

**Test scenarios:**
- `--dry-run` with `--target demo --channel 111...` completes without unknown-option error.

**Verification:** Manual or documents-scrape-smoke pattern; full proof requires auth (out of scope for smoke).

### U4. Full smoke gate

**Goal:** Confirm no regressions across operator chain.

**Requirements:** R6

**Files:** (none — verification only)

**Verification:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 20/20 (or current count) pass.

## Scope Boundaries

### In scope

- CLI passthrough on three remaining operator wrappers
- Targeted smoke updates

### Deferred to Follow-Up Work

- Stopping stale full-target validation processes
- Running live yes_general catch-up inside LFG
- Container memory tuning for large channels

## Assumptions

- Downstream `--channel` + single `--target` validation in `run-discord-scrape.sh` remains authoritative; wrappers do not re-validate channel membership.
