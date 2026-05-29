---
title: fix: Mixed-length snowflake cursor smoke
type: fix
status: complete
date: 2026-05-29
origin: /lfg — close residual review P2 on last_message_id; lock padded sort behavior
---

# fix: Mixed-length snowflake cursor smoke

## Summary

`last_message_id` already uses zero-padded `sort_by` (not string `max_by`). Add a smoke fixture where an 18-digit ID lexicographically beats a 19-digit ID but is numerically smaller, and assert `--after` uses the true maximum.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Fixture archive with IDs `999999999999999999` and `1000000000000000000` (unordered) |
| R2 | Smoke expects `--after` = `1000000000000000000` |
| R3 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes |
| R4 | PR #1538 notes residual P2 addressed |

## Verification

- `./scripts/tests/run-discord-scrape-smoke.sh`
- `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh`
