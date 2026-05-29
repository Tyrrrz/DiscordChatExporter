---
title: feat: Disk preflight on documents scrape entrypoint
type: feat
status: complete
date: 2026-05-29
origin: /lfg — plan 025 added disk checks to verify-operator-ready but run-documents-scrape bypassed them
---

# feat: Disk preflight on documents scrape entrypoint

## Summary

Operators often run `./scripts/run-documents-scrape.sh` directly (and monthly cron uses the host runner). Call the same archive disk check before any live Discord scrape so full disks fail fast with a clear message.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `verify-operator-ready.sh --disk-only` runs config parse + `require_archive_disk_space` only |
| R2 | `run-documents-scrape.sh` invokes disk check before preflight/scrape (not on `--dry-run`) |
| R3 | `documents-scrape-smoke.sh` covers `--disk-only` success path with `DCE_MIN_FREE_MB=0` |
| R4 | `run-all-smokes.sh` still passes |

## Verification

- `./scripts/tests/documents-scrape-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
