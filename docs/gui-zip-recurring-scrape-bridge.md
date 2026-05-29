# Recurring scrape — GUI zip users

The **DiscordChatExporter.linux-x64** (or similar) GUI zip is only the desktop app. Docker, cron, and append-only JSON merges live in the **source repo** checked out beside the zip:

```text
DiscordChatExporter.linux-x64/     ← GUI (this folder)
DiscordChatExporter/               ← source repo (scripts, Docker, config)
```

Sync to the GUI zip folder: `./scripts/sync-gui-bridge-doc.sh` (writes `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` by default).

## Quick start (run from source repo)

```bash
cd ../DiscordChatExporter   # or your clone path
./scripts/operator-handoff.sh              # disk + verify + archive dry-run
./scripts/sync-token-from-gui.sh --force   # token from GUI Settings.dat
# or: cp scrape.env.example scrape.env and set DISCORD_TOKEN (never commit)
./scripts/bootstrap-recurring-scrape.sh    # verify, build image, preflight
./scripts/run-documents-scrape.sh          # incremental append-only scrape
./scripts/run-operator-proof.sh --sync-gui --target eod_discord   # one-target handoff + scrape + grow-only proof
./scripts/setup-cron.sh --dry-run          # preview monthly job (default 1st @ 04:00)
./scripts/setup-cron.sh                    # install cron after handoff passes
```

Optional integrity tools:

```bash
./scripts/audit-archive-json.sh
# ./scripts/salvage-truncated-export.sh path/to/export.json
```

Archives: `config/scrape-targets.json` (typically `~/Documents/*` per target `output_dir`).

**Disk:** Free several GiB on `/home` and archive roots before large scrapes (`DCE_MIN_FREE_MB`, default 1024).

**Validate scripts:** `DCE_MIN_FREE_MB=0 ./scripts/run-all-smokes.sh` (19 offline smokes)

**Podman (Fedora):** install `podman-compose` when `docker compose` cannot reach the socket; scripts auto-prefer it.

## Wrappers in the GUI zip folder

From `DiscordChatExporter.linux-x64/`:

```bash
./bootstrap-recurring-scrape.sh --dry-run
```

That script delegates to `../DiscordChatExporter/scripts/bootstrap-recurring-scrape.sh`.

## More detail

- [Recurring-Scrape-Setup.md](../.docs/Recurring-Scrape-Setup.md)
- [recurring-scrape-operator-checklist.md](recurring-scrape-operator-checklist.md)
- [recurring-scrape-merge-readiness.md](recurring-scrape-merge-readiness.md)
