---
title: "feat: Optional container memory limit for large channel exports"
type: feat
status: complete
date: 2026-06-04
origin: /lfg — yes_general OOM repeatedly deferred; operators need a documented knob without changing default scrape behavior
---

# feat: Optional container memory limit for large channel exports

## Summary

Add `DCE_CONTAINER_MEMORY` so operators can raise the scrape container memory cap for multi-year catch-up channels like KotOR `yes_general` without affecting default runs (unlimited / runtime default when unset).

## Problem

`yes_general` (`221726893064454144`) legitimately fetches years of history on first catch-up. The .NET exporter inside the container OOMs on large in-memory JSON builds. Plans 043–051 preserved partial temps and salvage paths, but every full export retry still hits the same memory ceiling unless the operator manually tweaks Podman/Docker.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `docker-compose.yml` applies `mem_limit` from `DCE_CONTAINER_MEMORY` (0 = no compose cap) |
| R2 | `run-discord-scrape-host.sh` passes `DCE_CONTAINER_MEMORY` into compose env temp when set in shell or `scrape.env` |
| R3 | `scrape.env.example` documents `DCE_CONTAINER_MEMORY` with yes_general example (`8g`) |
| R4 | Operator docs mention the knob for large-channel catch-up |
| R5 | Host smoke asserts compose env receives `DCE_CONTAINER_MEMORY=8g` when configured |
| R6 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` passes (21/21) |

## Implementation Units

### U1. Compose memory limit wiring

**Files:** `docker-compose.yml`, `scripts/run-discord-scrape-host.sh`, `scrape.env.example`

- Service `mem_limit: ${DCE_CONTAINER_MEMORY:-0}` (0 = unlimited for Docker/Podman)
- `write_compose_env_temp` writes `DCE_CONTAINER_MEMORY` (explicit value or `0`)
- Host usage text documents env var

### U2. Operator documentation

**Files:** `docs/recurring-scrape-operator-checklist.md`, `docs/recurring-scrape-merge-readiness.md`

- yes_general section: set `DCE_CONTAINER_MEMORY=8g` (or host-appropriate) before channel-scoped validation
- Merge-readiness plan 063 stamp

### U3. Smoke coverage

**Files:** `scripts/tests/run-discord-scrape-host-smoke.sh`

- Fake compose logs loaded `DCE_CONTAINER_MEMORY` from compose env file
- Assert `8g` when set in scrape.env fixture

## Verification

```bash
./scripts/tests/run-discord-scrape-host-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
```

## Scope Boundaries

### Deferred

- Live KotOR catch-up execution inside LFG
- Per-channel memory overrides in `scrape-targets.json`
- Streaming export to avoid in-memory JSON (upstream DCE feature)
