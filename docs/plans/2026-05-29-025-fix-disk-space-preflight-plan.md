---
title: fix: Disk space preflight and skippable channel failures
type: fix
status: complete
date: 2026-05-29
origin: Repeated /lfg — full validation failed; /home at 100% capacity during KotOR export
---

# fix: Disk space preflight and skippable channel failures

## Summary

Host disk reached 100% during KotOR yes_general incremental export. Add archive-root free-space checks before scrape/validation and treat disk-full export errors as skippable channels so other channels in the same target still complete.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `require_archive_disk_space` in verify-operator-ready (configurable `DCE_MIN_FREE_MB`, default 2048) |
| R2 | `run-operator-validation.sh` calls disk check after readiness |
| R3 | `is_skippable_channel_export_failure` matches no-space / SQLITE_FULL / ENOSPC |
| R4 | Smoke: disk check fails when `DCE_MIN_FREE_MB` absurdly high |
| R5 | Document disk requirement in merge-readiness |

## Verification

- `./scripts/tests/verify-operator-ready-smoke.sh` or new disk smoke
- `./scripts/run-all-smokes.sh`
