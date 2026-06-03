---
title: "feat: Host runner recover JSON summary from compose log"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 071 deferred host-runner recovery when stdout is not teed to a persistent file
---

# feat: Host runner recover JSON summary from compose log

## Summary

`run-discord-scrape-host.sh` already tees compose output to a temporary run log. Before deleting that log on success, recover `DCE_RUN_SUMMARY_FILE` from the last `DCE_JSON_SUMMARY:` line when the file is missing or empty.

## Problem Frame

Operator validation (plan 071) recovers summaries from its teed log. Direct host scrapes (`run-discord-scrape-host.sh scrape`) capture compose stdout in a temp file but discard it after success. When the container logs `DCE_JSON_SUMMARY` but cannot write the mapped file, operators lose machine-readable totals unless they manually grep the scrollback.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | Shared `recover_json_summary_if_missing(run_log, dest_file)` skips when dest exists and is non-empty |
| R2 | `run_subcommand_with_retry` calls recovery on successful scrape/preflight runs before deleting the temp log |
| R3 | Recovery runs only when `DCE_RUN_SUMMARY_FILE` is set and file is missing or zero-length |
| R4 | Success prints `JSON summary recovered from run log:` to stderr |
| R5 | `run-operator-validation.sh` uses the shared helper instead of inline extract |
| R6 | Host smoke covers recovery from a synthetic run log |
| R7 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` → 22/22 |

## Implementation Units

### U1. Shared recovery helper

**Files:** `scripts/lib/scrape-summary-json.sh`, `scripts/tests/scrape-summary-json-smoke.sh`

**Approach:** Add `recover_json_summary_if_missing`; extend smoke with dest-already-exists skip case.

### U2. Host runner wiring

**Files:** `scripts/run-discord-scrape-host.sh`, `scripts/tests/run-discord-scrape-host-smoke.sh`

**Approach:** Source lib in host runner; call recovery before `rm -f "$output_file"` on both success paths in `run_subcommand_with_retry`.

### U3. Validation refactor + docs

**Files:** `scripts/run-operator-validation.sh`, `docs/recurring-scrape-merge-readiness.md`

**Approach:** Replace inline extract block with shared helper; add Plan 072 stamp.

## Verification

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up on host
- Auto-enable JSON summary on bare `host.sh scrape` without env vars
- Operator-proof JSON summary parity
- Merging multiple per-target summaries into one JSON artifact
