---
title: fix: Verify Documents targets and bootstrap scrape auth
type: fix
status: completed
date: 2026-05-28
origin: LFG repeat — confirm ~/Documents/** append paths and unblock auth setup
depends_on: docs/plans/2026-05-28-006-fix-documents-append-auth-plan.md
---

# fix: Verify Documents targets and bootstrap scrape auth

## Summary

Plan 006 landed append-safe scraping. This pass adds operator tooling so you can (1) verify every enabled target maps to an on-disk `~/Documents/<server>/` tree with seeded channel archives, and (2) create `scrape.env` without manual file editing when a token is already exported.

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| V1 | `verify-documents-archives.sh` reports per-target output_dir, JSON count, seeded channel IDs, channel-map coverage | `scripts/verify-documents-archives.sh`, smoke test |
| V2 | `setup-scrape-auth.sh` writes `scrape.env` from `DISCORD_TOKEN` or `DISCORD_TOKEN_FILE` (chmod 600), idempotent | `scripts/setup-scrape-auth.sh`, smoke test |
| V3 | Document verify + auth bootstrap in setup guide | `.docs/Recurring-Scrape-Setup.md` |

## Success Criteria

- Verify script runs against real `config/scrape-targets.json` and exits 0 when enabled targets have archive dirs
- Auth bootstrap creates scrape.env when token env vars are set
- Smoke tests pass
