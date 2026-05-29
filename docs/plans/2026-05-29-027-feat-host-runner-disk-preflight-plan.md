---
title: feat: Disk preflight on host runner (cron path)
type: feat
status: complete
date: 2026-05-29
origin: /lfg — monthly cron calls run-discord-scrape-host.sh directly, bypassing run-documents-scrape disk check
---

# feat: Disk preflight on host runner (cron path)

## Summary

`setup-cron.sh` invokes `run-discord-scrape-host.sh scrape`, not `run-documents-scrape.sh`. Run the same `--disk-only` check in the host wrapper before `preflight` and `scrape` so scheduled jobs fail fast when `/home` is full.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-discord-scrape-host.sh` resolves host config from `--config` (maps `/config/...` to repo config) |
| R2 | Calls `verify-operator-ready.sh --disk-only` when the script exists (skips in minimal fake-repo smokes) |
| R3 | `DCE_SKIP_DISK_CHECK=1` bypasses check for tests that need it |
| R4 | `run-all-smokes.sh` still passes with `DCE_MIN_FREE_MB=0` |

## Verification

- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
