---
title: "feat: Pass --channel through operator scrape wrappers"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — yes_general catch-up needs single-channel runs; host runner already supports --channel but documents/validation wrappers reject it
---

# feat: Pass --channel through operator scrape wrappers

## Problem

`run-discord-scrape.sh` and `run-discord-scrape-host.sh` accept repeatable `--channel ID` (requires exactly one `--target`), but `run-documents-scrape.sh` and `run-operator-validation.sh` die on unknown `--channel`. Operators re-running KotOR `yes_general` (`221726893064454144`) must scrape all guild channels instead of narrowing to one backlog channel.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-documents-scrape.sh` accepts repeatable `--channel ID` and forwards to host preflight/scrape |
| R2 | `run-operator-validation.sh` accepts repeatable `--channel ID` and forwards to documents scrape |
| R3 | Usage text documents `--channel` requires exactly one `--target` (enforced downstream) |
| R4 | Smoke asserts `--channel` is accepted (dry-run path) and forwarded (fake-docker arg capture) |
| R5 | `run-all-smokes.sh` passes |

## Implementation

- `scripts/run-documents-scrape.sh` — parse `--channel`, append to `passthrough`
- `scripts/run-operator-validation.sh` — parse `--channel`, append to `scrape_args` / per-target args
- `scripts/tests/documents-scrape-smoke.sh` — dry-run with `--channel`; optional arg-capture via fake host

## Verification

```bash
./scripts/tests/documents-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- Container memory limits for yes_general
- Completing yes_general catch-up inside LFG
- Changing core scrape channel validation logic
