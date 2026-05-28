---
title: fix: Live Documents scrape proof and token discovery
type: fix
status: completed
date: 2026-05-28
origin: LFG repeat — ensure ~/Documents/** append scrape works with proper auth
depends_on: docs/plans/2026-05-28-007-verify-documents-auth-bootstrap-plan.md
---

# fix: Live Documents scrape proof and token discovery

## Summary

Append-safe scraping is implemented but live Discord auth has never been exercised in this environment. Add automatic token-file discovery, a unified operator entrypoint, and a grow-only proof harness that records message counts before/after a scrape.

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| L1 | Host runner discovers `DISCORD_TOKEN_FILE` from standard paths when unset | `scripts/run-discord-scrape-host.sh`, smoke test |
| L2 | `run-documents-scrape.sh` runs verify → auth check → preflight → scrape | `scripts/run-documents-scrape.sh` |
| L3 | `prove-incremental-append.sh` asserts same paths and non-shrinking message counts | `scripts/prove-incremental-append.sh`, smoke test |

## Success Criteria

- `./scripts/run-documents-scrape.sh --dry-run` passes without token
- With valid token, `./scripts/prove-incremental-append.sh --target KotOR_discord_msgs` shows grow-only counts
- Smoke tests pass
