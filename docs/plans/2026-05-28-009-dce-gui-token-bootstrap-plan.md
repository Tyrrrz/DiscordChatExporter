---
title: fix: Bootstrap Discord auth from DCE GUI Settings.dat
type: fix
status: completed
date: 2026-05-28
origin: LFG — live Documents scrape blocked without token; GUI Settings.dat exists locally
depends_on: docs/plans/2026-05-28-008-live-documents-scrape-proof-plan.md
completed: 2026-05-28
---

# fix: Bootstrap Discord auth from DCE GUI Settings.dat

## Summary

Append-safe Documents scraping is implemented and archives verify cleanly. Live runs now authenticate via `discover-discord-token.sh` (Discord desktop leveldb, optional GUI Settings.dat decrypt), mount the host scrape script in compose (so preflight uses `--partition 1` + `--after` instead of stale `--before 1970-01-01`), and skip forbidden/inaccessible channels without aborting the whole target.

## Additional requirements (landed with compose mount + resilience)

| ID | Requirement | Files |
|----|-------------|-------|
| G5 | Mount host `run-discord-scrape.sh` into container | `docker-compose.yml`, `scripts/tests/container-smoke.sh` |
| G6 | Preflight uses partition + optional `--after` cursor (no epoch `--before`) | `scripts/run-discord-scrape.sh` |
| G7 | Skip forbidden/not-found channels; continue scrape | `scripts/run-discord-scrape.sh`, smoke test |

## Problem Frame

- **In scope:** Discover `Settings.dat`, decrypt `LastToken` with the same PBKDF2/AES-GCM scheme as `SettingsService.TokenEncryptionConverter`, integrate into host runner token discovery, document path env vars, smoke test decrypt (without printing token), run one live incremental scrape + grow-only proof on a seeded target.
- **Out of scope:** Committing tokens, browser-based reauth flows, changing merge/append logic (already landed in 006–008).

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| G1 | `read-dce-gui-token` decrypts `LastToken` from Settings.dat (enc + plain) | `scripts/tools/ReadDceGuiToken/*`, `scripts/read-dce-gui-token.sh` |
| G2 | Host runner discovers Settings.dat and loads token when no explicit env/file | `scripts/run-discord-scrape-host.sh`, smoke test |
| G3 | Docs mention `DISCORDCHATEXPORTER_SETTINGS_PATH` and sibling `linux-x64/Settings.dat` | `.docs/Recurring-Scrape-Setup.md`, `scrape.env.example` |
| G4 | Live proof: preflight + scrape + grow-only harness on one enabled target | operator run (not committed) |

## Decisions

- Use a tiny `dotnet` console tool (BCL only) instead of Python `cryptography` to avoid venv/PEP 668 friction on Fedora.
- Machine ID resolution mirrors GUI: `/etc/machine-id`, `/var/lib/dbus/machine-id`, then `Environment.MachineName`.
- Token never logged; decrypt writes only to stdout for shell capture or mode-600 temp file inside host runner.

## Test Scenarios

| Scenario | Expected |
|----------|----------|
| Settings.dat with `enc:` token on same machine | decrypt exits 0, non-empty stdout |
| Missing Settings.dat | discover skips, existing error message unchanged |
| `--dry-run` | still passes without decrypt |
| Live scrape on seeded target | same JSON paths, message count ≥ before |

## Implementation Units

1. **ReadDceGuiToken tool** — `scripts/tools/ReadDceGuiToken/Program.cs`, `.csproj`, shell wrapper
2. **Host discovery integration** — extend `discover_token_file` / `ensure_token_present`
3. **Docs + smoke** — update setup doc, add host smoke case with fixture Settings.dat (plain token for test)
