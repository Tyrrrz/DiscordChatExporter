---
title: "feat: Preserve-partial smoke and KotOR validation run"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — close plan 043 with regression smoke; run live KotOR validation
---

# feat: Preserve-partial smoke and KotOR validation run

## Summary

Plan 043 fixed the re-download loop but lacks offline regression for "preserve partial temp on OOM skip". Add smoke coverage, rebuild container, run KotOR validation, update merge-readiness.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Smoke: fake CLI writes partial export for channel 134 then exits 134; temp dir preserved after SKIPPED |
| R2 | `run-discord-scrape-smoke.sh` and `run-all-smokes.sh` pass (19/19) |
| R3 | Rebuild image; start `run-operator-validation.sh --target KotOR_discord_msgs` with log |
| R4 | `docs/recurring-scrape-merge-readiness.md` updated with validation run status |
| R5 | PR #1538 body notes plan 044 |

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
podman-compose build
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --target KotOR_discord_msgs --log-file logs/kotor-validation-20260604.log
```

## Out of scope

- Waiting for yes_general multi-hour catch-up to finish inside LFG
- Container memory tuning
