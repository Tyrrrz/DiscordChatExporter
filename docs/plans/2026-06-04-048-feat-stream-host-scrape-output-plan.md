---
title: "feat: Stream container scrape output during host runs"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — KotOR validation log frozen at ~83 lines while yes_general export ran for hours
---

# feat: Stream container scrape output during host runs

## Problem

`run-discord-scrape-host.sh` captures all container stdout/stderr into a temp file and only `cat`s it after the compose run exits. Long exports (e.g. KotOR `yes_general`) leave operator validation logs silent for hours even though the container is actively exporting.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run_subcommand_with_retry` streams compose output to stdout as it arrives while still capturing to the temp file for auth-failure detection |
| R2 | Preserve exit-code semantics and auth-retry behavior (pipefail + `PIPESTATUS[0]`) |
| R3 | Do not duplicate full output on success (tee replaces post-hoc `cat`) |
| R4 | Host smoke adds a `streaming` fake-docker mode proving first line appears before command completes |
| R5 | `run-all-smokes.sh` passes |

## Implementation

- **File:** `scripts/run-discord-scrape-host.sh` — replace `>"$output_file" 2>&1` + `cat` with `"${run_args[@]}" 2>&1 | tee "$output_file"` and check `${PIPESTATUS[0]}` in both initial and retry paths.
- **File:** `scripts/tests/run-discord-scrape-host-smoke.sh` — add streaming mode assertion.

## Verification

```bash
./scripts/tests/run-discord-scrape-host-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- yes_general catch-up completion
- Container memory limits
- Validation-level flock (host flock already exists)
