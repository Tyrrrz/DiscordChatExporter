---
title: "fix: Preserve partial exports on OOM skip; large-file salvage"
type: fix
status: complete
date: 2026-06-03
origin: /lfg — yes_general re-downloads because OOM skip deletes partial temp; salvage fails on 500MB+ JSON
---

# fix: Preserve partial exports on OOM skip; large-file salvage

## Problem

1. **OOM skip discards progress:** When export exits 134/137/139, `scrape_target` SKIPs the channel and `rm -rf`s the temp dir — losing partial downloads (514 MB, 1 GB).
2. **Salvage fails on large files:** Python marker salvage + `jq empty` on 500 MB+ truncated JSON fails in container (`mktemp` / memory).
3. **Re-download loop:** Stale temps discarded → incremental starts from 2021 archive cursor → 35+ min re-fetch every run.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | On SKIPPED export (exit 2), **do not** delete temp dir — leave for next-run salvage |
| R2 | `salvage_truncated_json` uses grep/head boundary repair; mktemp uses `${TMPDIR:-/tmp}` |
| R3 | Skip full-file `jq empty` on exports > 64 MiB; validate via python message-count probe |
| R4 | Large merge (>64 MiB combined) uses python id-merge instead of jq |
| R5 | Smoke tests pass; salvage-stale smoke unchanged |
| R6 | Salvage current 1 GB yes_general temp, merge into archive, verify `--after` advances |

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
# After merge, incremental should show recent dateRange.after not 2021
```

