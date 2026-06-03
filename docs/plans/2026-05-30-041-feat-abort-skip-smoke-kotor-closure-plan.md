---
title: feat: Abort-skip smoke and KotOR validation closure
type: feat
status: complete
date: 2026-05-30
origin: /lfg — close plan 040 R3/R4 with offline abort smoke and KotOR retry
---

# feat: Abort-skip smoke and KotOR validation closure

## Summary

Plan 040 landed OOM/abort channel skip (exit 134/137/139) but lacked offline regression coverage and KotOR host re-validation. Add a fake-CLI abort case to `run-discord-scrape-smoke.sh`, re-run KotOR validation after image rebuild, and stamp merge-readiness when 9/9 or documented skip.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `run-discord-scrape-smoke.sh` covers channel export exit 134 (abort) — target completes, accessible channel merges |
| R2 | `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` — 19/19 pass |
| R3 | Rebuild compose image; `run-operator-validation.sh --target KotOR_discord_msgs` completes (may skip `yes_general`) |
| R4 | `docs/recurring-scrape-merge-readiness.md` updated with 2026-05-30 closure and KotOR row |
| R5 | PR #1538 body notes plan 041 closure |

## Files

- `scripts/tests/run-discord-scrape-smoke.sh` — fake CLI channel `134` exits 134; `skip-abort` target
- `docs/recurring-scrape-merge-readiness.md` — host validation table
- `docs/plans/2026-05-30-040-fix-skip-oom-channel-export-plan.md` — mark R3/R4 complete when done

## Test scenarios (R1)

1. Two-channel target: channel `111` exports; channel `134` fake CLI exits 134 with abort stderr
2. Scrape exits 0; destination for `111` has appended messages
3. No fallback `channels/134.json` created
4. Run summary includes SKIPPED for channel 134

## Verification

```bash
./scripts/tests/run-discord-scrape-smoke.sh
DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh
docker compose build   # or podman-compose build
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --target KotOR_discord_msgs \
  --log-file logs/kotor-retry-20260530.log
```

## Risks

- `yes_general` may take ~35 min before abort; validation log is authoritative
- Large merges need ~22 GiB free on `/home`

## Out of scope

- Increasing container memory for full `yes_general` export
- New `/lfg` feature work beyond closure
