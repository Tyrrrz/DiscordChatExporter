---
title: "feat: Host compose passthrough for JSON scrape summary"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 069 deferred host compose passthrough; container runs could not write DCE_RUN_SUMMARY_FILE on host
---

# feat: Host compose passthrough for JSON scrape summary

## Summary

Mount repo `logs/` into the scrape container, pass `DCE_RUN_SUMMARY_*` through compose env, map host `logs/*.json` paths to `/logs/*`, and auto-enable JSON summary in `run-operator-validation.sh` when scraping.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docker-compose.yml` mounts `./logs:/logs:z` |
| R2 | `write_compose_env_temp` passes `DCE_RUN_SUMMARY_JSON` and mapped `DCE_RUN_SUMMARY_FILE` |
| R3 | Host paths under `$REPO_ROOT/logs/` map to `/logs/<basename>` inside container |
| R4 | `run-operator-validation.sh` sets `DCE_RUN_SUMMARY_JSON=1` and `*.summary.json` beside `--log-file` when scraping |
| R5 | Host smoke asserts summary env passthrough and `/logs/` mapping |
| R6 | Validation smoke asserts dry-run does not enable JSON summary |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. Compose mount + host passthrough

**Files:** `docker-compose.yml`, `scripts/run-discord-scrape-host.sh`, `scripts/run-operator-validation.sh`, smokes

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Auto-extract JSON from tee log when file write fails
