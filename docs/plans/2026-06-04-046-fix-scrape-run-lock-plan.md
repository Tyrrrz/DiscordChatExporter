---
title: "fix: Scrape run lock prevents concurrent container exports"
type: fix
status: complete
date: 2026-06-04
origin: /lfg — duplicate KotOR validation runs left two yes_general exports OOM-looping
---

# fix: Scrape run lock prevents concurrent container exports

## Problem

Two overlapping `run-operator-validation.sh --target KotOR_discord_msgs` processes each started a full container scrape. Both exported `yes_general` (`221726893064454144`) with the same `--after` cursor, creating twin `.dce-temp/export.*` dirs (~29–34 MiB each) and repeated OOM skips.

Cron uses `flock`, but manual/host validation does not — overlapping runs are unguarded.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-discord-scrape-host.sh scrape` acquires non-blocking `flock` on `$REPO_ROOT/.dce-scrape.lock` |
| R2 | `DCE_SKIP_SCRAPE_LOCK=1` bypasses lock (smoke tests) |
| R3 | Clear error when lock held; preflight unaffected |
| R4 | Offline smoke asserts second scrape fails while lock held |
| R5 | `run-all-smokes.sh` passes (19/19); docs note concurrent-run hazard |

## Verification

```bash
./scripts/tests/run-discord-scrape-host-lock-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- Completing yes_general multi-hour catch-up inside LFG
- Container memory limits / tuning
