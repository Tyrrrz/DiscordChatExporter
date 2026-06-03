---
title: "feat: Recover JSON scrape summary from tee log"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 070 deferred auto-extract when DCE_RUN_SUMMARY_FILE write fails inside container
---

# feat: Recover JSON scrape summary from tee log

## Summary

When operator validation enables JSON summary export but the container does not write `DCE_RUN_SUMMARY_FILE`, recover the summary from the last `DCE_JSON_SUMMARY:` line in the teed validation log.

## Problem Frame

Plan 070 mounted `logs/` and mapped summary paths for compose runs. File writes can still fail (permissions, missing mount on ad-hoc runs, partial compose failures). The scrape script always logs `DCE_JSON_SUMMARY:` when `DCE_RUN_SUMMARY_JSON=1`, and operator validation tees all output to `--log-file`. A host-side fallback avoids losing machine-readable totals.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Shared helper extracts compact JSON after the last `DCE_JSON_SUMMARY:` prefix in a log file |
| R2 | Helper validates JSON with `jq` and writes pretty-printed output to the destination path |
| R3 | `run-operator-validation.sh` invokes fallback when JSON export enabled and summary file missing or empty after tee completes |
| R4 | Recovery success logs `JSON summary recovered from log:` with the file path |
| R5 | Offline smoke covers extract-from-log without live Discord |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 21/21 |

## Key Technical Decisions

- **Last line wins:** Multiple scrapes in one validation run may emit several summaries; use the last `DCE_JSON_SUMMARY:` line (most recent scrape totals).
- **No overwrite:** Only recover when destination file is missing or zero-length; do not replace an existing valid file.

## Implementation Units

### U1. Extract helper library

**Goal:** Reusable log-line recovery for JSON summaries.

**Files:** `scripts/lib/scrape-summary-json.sh`, `scripts/tests/scrape-summary-json-smoke.sh`

**Approach:** `extract_json_summary_from_log(source_log dest_file)` greps for `DCE_JSON_SUMMARY:`, takes the last match, strips prefix, `jq .` to dest. Return 0 on success, 1 when no line or invalid JSON.

**Test scenarios:**
- Happy path: log with `[timestamp] DCE_JSON_SUMMARY: {"version":1,...}` → valid pretty JSON file
- No marker line → returns 1, dest unchanged
- Invalid JSON after prefix → returns 1

**Verification:** smoke script passes standalone.

### U2. Wire into operator validation

**Goal:** Auto-recover after teed validation when file write failed.

**Dependencies:** U1

**Files:** `scripts/run-operator-validation.sh`, `scripts/tests/run-operator-validation-smoke.sh`

**Approach:** Source lib after tee; if `export_json_summary` and `DCE_RUN_SUMMARY_FILE` set and file not `-s`, call extract; log recovery on success.

**Test scenarios:**
- Smoke simulates log-only summary (write fake log + call helper path, or dry-run skip unchanged)
- Existing dry-run smoke still asserts no JSON summary path logged

**Verification:** operator-validation smoke passes.

### U3. Docs stamp

**Goal:** Record plan 071 in merge-readiness.

**Files:** `docs/recurring-scrape-merge-readiness.md`

**Approach:** Add Plan 071 bullet; refresh stale KotOR block (lines 147–153) to cite per-target `container_memory: "8g"` and channel-scoped validation with `.summary.json`.

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Host runner post-scrape recovery when stdout is not teed to a file
- Merging multiple per-target summaries into one JSON artifact
