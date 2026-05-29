---
title: feat: Operator-ready verification script
type: feat
status: complete
date: 2026-05-29
origin: Repeated /lfg — close loop with host prerequisite checks before cron/scrape
---

# feat: Operator-ready verification script

## Summary

Add `verify-operator-ready.sh` to check jq, container runtime, auth, config, and seeded archives in one command. Wire into docs and a smoke test.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scripts/verify-operator-ready.sh` checks jq, docker compose or podman compose, token presence, valid config |
| R2 | Reports per enabled target: output_dir exists, JSON count, channel-map status |
| R3 | `--preflight TARGET` optional single-target Discord preflight |
| R4 | `scripts/tests/verify-operator-ready-smoke.sh` offline smoke |
| R5 | Document in merge-readiness and operator checklist |

## Verification

- `./scripts/tests/verify-operator-ready-smoke.sh`
- `./scripts/run-all-smokes.sh`
