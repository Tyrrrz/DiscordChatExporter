---
title: fix: Ensure Documents archive paths append safely with auth
type: fix
status: completed
date: 2026-05-28
origin: User request — extract to ~/Documents/** per server, append not overwrite, proper CLI auth
---

# fix: Ensure Documents archive paths append safely with auth

## Summary

Recurring scrapes must update the user's existing large JSON archives under `~/Documents/<server>/` in place using DiscordChatExporter incremental export (`--after`) and merge-by-id, never replacing a file with a fresh full export when an archive already exists. Auth must work without fragile manual setup.

## Problem Frame

| Gap | Impact |
|-----|--------|
| `scrape.env` required even when `DISCORD_TOKEN` is already exported | Preflight/scrape fail before auth is attempted |
| Channel map not bootstrapped from existing `* [id].json` files | Risk of creating parallel files instead of updating in place |
| Merge replaces destination via direct `mv` without monotonic guard | Large archives could shrink on bad merge |
| `OpenKotOR_discord_msgs` target points at missing folder | Target resolves zero channels while `openkotor_discord_msgs` holds data |

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| U1 | Make host runner accept exported `DISCORD_TOKEN` / `DISCORD_TOKEN_FILE` when `scrape.env` is absent | `scripts/run-discord-scrape-host.sh`, smoke test |
| U2 | Bootstrap `output_dir/.dce-meta/channel-map.json` from existing `* [channel_id].json` archives before scrape/preflight | `scripts/run-discord-scrape.sh`, smoke test |
| U3 | Safe merge: verify merged message count ≥ existing; replace via temp file in target directory | `scripts/run-discord-scrape.sh`, smoke test |
| U4 | Align config with on-disk folders (disable missing OpenKotOR target) | `config/scrape-targets.json` |
| U5 | Document auth + in-place append contract | `.docs/Recurring-Scrape-Setup.md` |

## Test Scenarios

- Host runner succeeds with only `DISCORD_TOKEN` in environment (no scrape.env)
- Bootstrap writes channel-map entries for seeded archives without overwriting map entries
- Merge rejects shrinkage (fixture with fewer messages after merge)
- Existing smoke suite still passes

## Success Criteria

- `./scripts/tests/run-discord-scrape-smoke.sh` and host smoke pass
- Preflight can run once user exports token (even without scrape.env file)
