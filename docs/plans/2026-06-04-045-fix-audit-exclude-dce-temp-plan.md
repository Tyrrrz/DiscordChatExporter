---
title: "fix: Exclude .dce-temp from archive JSON audit"
type: fix
status: complete
date: 2026-06-04
origin: /lfg — KotOR validation audit fails on in-progress partial exports in .dce-temp
---

# fix: Exclude .dce-temp from archive JSON audit

## Problem

`audit-archive-json.sh` scans every `*.json` under a target output dir. Partial/incomplete exports in `.dce-temp/export.*` (truncated mid-catch-up) fail `jq empty`, so operator validation reports audit failure even when archive files are valid.

Observed: `INVALID .../KotOR_discord_msgs/.dce-temp/export.221726893064454144.kbMFiP/export.json` after yes_general OOM skip preserved partial temp (plan 043).

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `audit-archive-json.sh` skips files under `*/.dce-temp/*` (same as `.dce-meta`) |
| R2 | `verify-documents-archives.sh` uses consistent exclusion if it scans JSON |
| R3 | `audit-archive-json-smoke.sh` covers a fixture partial under `.dce-temp` that must not fail audit |
| R4 | `run-all-smokes.sh` passes (19/19) |
| R5 | KotOR audit passes while partial temp exists; update `docs/recurring-scrape-merge-readiness.md` |

## Verification

```bash
./scripts/tests/audit-archive-json-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
./scripts/audit-archive-json.sh --config config/scrape-targets.json --target KotOR_discord_msgs
```

## Out of scope

- Completing yes_general multi-hour catch-up inside LFG
- Container memory tuning
