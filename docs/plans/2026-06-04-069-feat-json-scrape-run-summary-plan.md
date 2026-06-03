---
title: "feat: Optional JSON scrape run summary"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 038 deferred structured JSON run logs for operator validation and automation
---

# feat: Optional JSON scrape run summary

## Summary

When `DCE_RUN_SUMMARY_JSON=1` and/or `DCE_RUN_SUMMARY_FILE` is set, emit a machine-readable scrape summary alongside the existing human log summary in `run-discord-scrape.sh`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | JSON includes version, finished_at, totals, and per-channel entries matching the text summary |
| R2 | `DCE_RUN_SUMMARY_JSON=1` logs one `DCE_JSON_SUMMARY:` line (compact JSON) |
| R3 | `DCE_RUN_SUMMARY_FILE` writes pretty-printed JSON when parent dir exists |
| R4 | `scrape.env.example` documents both env vars |
| R5 | `run-discord-scrape-smoke.sh` asserts valid JSON file with merged channel |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 21/21 |

## Implementation Units

### U1. JSON writer in run-discord-scrape.sh

**Files:** `scripts/run-discord-scrape.sh`, `scripts/tests/run-discord-scrape-smoke.sh`, `scrape.env.example`

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Host compose passthrough of summary file path (operators can grep `DCE_JSON_SUMMARY`)
