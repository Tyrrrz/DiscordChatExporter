---
title: docs: LFG closure after operator proof (plan 032)
type: docs
status: complete
date: 2026-05-29
origin: /lfg — verify 19 smokes, sync GUI bridge with run-operator-proof, refresh PR
---

# docs: LFG closure after operator proof (plan 032)

## Summary

Plan 032 landed `run-operator-proof.sh` and the Podman smoke fix. This slice aligns operator/GUI docs with the new proof path, re-syncs the linux-x64 bridge, and re-runs the offline smoke gate before PR refresh.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docs/gui-zip-recurring-scrape-bridge.md` documents `run-operator-proof.sh` |
| R2 | `scripts/sync-gui-bridge-doc.sh` copies updated bridge to `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` |
| R3 | `.docs/Recurring-Scrape-Setup.md` mentions operator proof in quick path |
| R4 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes (19 scripts) |
| R5 | PR #1538 body notes plan 033 / smoke count |

## Verification

- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
- `./scripts/sync-gui-bridge-doc.sh`
- `./scripts/run-operator-proof.sh --dry-run` (host, when config present)
