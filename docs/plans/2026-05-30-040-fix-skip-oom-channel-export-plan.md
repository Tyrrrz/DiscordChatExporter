---
title: fix: Skip OOM/aborted channel exports
type: fix
status: complete
date: 2026-05-30
origin: /lfg — KotOR yes_general CLI core dump should not fail entire target
---

# fix: Skip OOM/aborted channel exports

## Summary

`yes_general` export aborted with `Aborted (core dumped)` inside the container, failing the whole `KotOR_discord_msgs` target. Extend skippable export failures so OOM/abort/kill errors skip the channel and continue (like forbidden channels).

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `is_skippable_channel_export_failure` matches Aborted, core dumped, OOM, Killed |
| R2 | `run-discord-scrape-smoke.sh` still passes |
| R3 | Re-run `run-operator-validation.sh --target KotOR_discord_msgs` completes (may skip yes_general) |
| R4 | Update merge-readiness KotOR row; 19 smokes pass |

## Verification

- `./scripts/tests/run-discord-scrape-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
- `logs/validation-resume-20260530.log` or new log for KotOR pass
