---
title: feat: Live operator proof on smallest target
type: feat
status: complete
date: 2026-05-29
origin: /lfg — validate end-to-end scrape on host after Podman compose fix
---

# feat: Live operator proof on smallest target

## Summary

Run `run-operator-proof.sh` against `eod_discord` on the host (sync GUI token, Podman compose). Fix any blocking issues discovered. Re-run offline smokes before push.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes |
| R2 | `./scripts/run-operator-proof.sh --sync-gui --target eod_discord` completes on host |
| R3 | Document host outcome in merge-readiness (pass/fail + fix applied) |
| R4 | Push branch; refresh PR #1538 |

## Verification

- Offline smokes (19 scripts)
- Live operator proof log under `logs/operator-proof-*.log`
