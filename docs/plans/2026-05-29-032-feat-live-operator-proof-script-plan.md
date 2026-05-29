---
title: feat: Live operator proof script for one target
type: feat
status: complete
date: 2026-05-29
origin: /lfg — handoff passes; prove append-only scrape on host for smallest enabled target
---

# feat: Live operator proof script for one target

## Summary

Add `scripts/run-operator-proof.sh` to run handoff, incremental scrape, and grow-only proof for a single configured target (default `eod_discord`). Validates the full operator path on the host after LFG implementation.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `--target NAME` (default `eod_discord`), `--sync-gui`, `--dry-run` (handoff only) |
| R2 | Runs `operator-handoff.sh`, then `run-documents-scrape.sh`, then `prove-incremental-append.sh` |
| R3 | Logs to `logs/operator-proof-TIMESTAMP.log` |
| R4 | `run-operator-proof-smoke.sh` uses `--dry-run` with fixture config |
| R5 | Host run for `eod_discord` when token available |
| R6 | Prefer `podman-compose` in host runner when installed (Fedora/Podman socket) |

## Verification

- `./scripts/tests/run-operator-proof-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
- `./scripts/run-operator-proof.sh --target eod_discord` on host (manual)
