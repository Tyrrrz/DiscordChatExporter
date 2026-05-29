---
title: feat: Unified smoke runner and validation docs
type: feat
status: complete
date: 2026-05-29
origin: Repeated /lfg — consolidate test entrypoint and align docs with CI
---

# feat: Unified smoke runner and validation docs

## Summary

Recurring scrape is feature-complete. Add one authoritative smoke runner script, wire CI to it, and update setup docs for audit/salvage/prove offline modes.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scripts/run-all-smokes.sh` runs all `scripts/tests/*.sh` except `container-smoke.sh` by default |
| R2 | `--include-container` runs container smoke for local Docker/Podman validation |
| R3 | CI `recurring-scrape-smoke` job uses `run-all-smokes.sh` |
| R4 | `.docs/Recurring-Scrape-Setup.md` smoke table lists all 12 smokes + audit/salvage/prove notes |
| R5 | Operator checklist references `run-all-smokes.sh` |

## Verification

- `./scripts/run-all-smokes.sh`
- `./scripts/run-all-smokes.sh --include-container` when Docker available (optional local)
