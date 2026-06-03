---
title: "feat: Allocate compose TTY for live operator scrape logs"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — plan 048 host tee still shows frozen validation logs; yes_general temp grows but podman run -T block-buffers CLI progress
---

# feat: Allocate compose TTY for live operator scrape logs

## Problem

Plan 048 streams host-side compose output via `tee`, but `compose_run_args` always passes `-T` (no pseudo-TTY). Containerized DiscordChatExporter progress lines stay block-buffered when piped, so operator validation logs remain silent for hours during large exports.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `compose_run_args` omits `-T` when `DCE_COMPOSE_TTY` is unset or non-zero; passes `-T` only when `DCE_COMPOSE_TTY=0` |
| R2 | `setup-cron.sh` cron job sets `DCE_COMPOSE_TTY=0` (non-interactive log append) |
| R3 | Host smoke asserts default omits `-T` and `DCE_COMPOSE_TTY=0` passes `-T` |
| R4 | Cron smoke asserts installed job includes `DCE_COMPOSE_TTY=0` |
| R5 | `run-all-smokes.sh` passes |

## Implementation

- `scripts/run-discord-scrape-host.sh` — `compose_tty_flag()` + use in all compose branches; document env in usage
- `scripts/setup-cron.sh` — prefix cron job with `DCE_COMPOSE_TTY=0`
- `scripts/tests/run-discord-scrape-host-smoke.sh` — compose arg capture for `-t`/`-T`
- `scripts/tests/setup-cron-smoke.sh` — grep crontab for `DCE_COMPOSE_TTY=0`

## Verification

```bash
./scripts/tests/run-discord-scrape-host-smoke.sh
./scripts/tests/setup-cron-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Out of scope

- yes_general catch-up completion
- Container memory limits
