# Recurring scrape operator checklist

Use this after cloning or opening the **source** repo (`DiscordChatExporter`, not the GUI zip alone).

## One-time setup

0. `./scripts/operator-handoff.sh` — recommended: disk summary, verify-operator-ready, and documents dry-run in one step.
   Or `./scripts/verify-operator-ready.sh` alone for prerequisites only.
1. `cp scrape.env.example scrape.env` and set `DISCORD_TOKEN`, or `./scripts/sync-token-from-gui.sh --force` (reads GUI `Settings.dat`).
2. `./scripts/bootstrap-recurring-scrape.sh --dry-run` — confirm every **enabled** target has seeded JSON under `output_dir`.
3. `./scripts/bootstrap-recurring-scrape.sh` — verify archives, build image, preflight Discord.
4. `./scripts/run-documents-scrape.sh` — first incremental append-only scrape.
   Or `./scripts/run-operator-proof.sh --sync-gui --target <name>` — handoff + scrape + grow-only proof in one step.
5. `./scripts/prove-incremental-append.sh --target <name>` — optional if you did not use `run-operator-proof.sh`.
6. `./scripts/audit-archive-json.sh` — optional; lists invalid JSON before cron runs.

## Monthly automation

```bash
./scripts/setup-cron.sh --dry-run
./scripts/setup-cron.sh --skip-preflight   # after bootstrap preflight already succeeded
```

Defaults: first day of month at 04:00. Override with `--interval weekly`, `--at HH:MM`, or `--cron '0 4 1 * *'`.

Installed jobs are marked `# BEGIN discord-scrape` in `crontab -l`. Logs append to `logs/discord-scrape.log`.

## Narrow a run

```bash
./scripts/run-documents-scrape.sh --target KotOR_discord_msgs
./scripts/run-documents-scrape.sh --target KotOR_discord_msgs --channel CHANNEL_ID
./scripts/setup-cron.sh --target KotOR_discord_msgs --channel CHANNEL_ID
```

## Scrape lock and salvage

Only one scrape should run per `archive_root`. Lock file: `{archive_root}/.dce-scrape.lock`.

```bash
./scripts/scrape-lock-status.sh
./scripts/scrape-lock-status.sh --reclaim-stale   # after crashed run; only when stale/free
```

Salvage partial exports under `output_dir/.dce-temp/` without calling Discord:

```bash
./scripts/operator-handoff.sh --salvage-only --target NAME [--channel ID]
./scripts/run-documents-scrape.sh --salvage-only --target NAME [--channel ID]
./scripts/run-operator-validation.sh --salvage-only --target NAME [--channel ID] --log-file logs/salvage.log
```

Salvage then incremental scrape:

```bash
./scripts/run-documents-scrape.sh --salvage-before-scrape --target NAME [--channel ID]
./scripts/run-operator-validation.sh --salvage-before-scrape --target NAME [--channel ID] --log-file logs/scrape.log
./scripts/run-operator-proof.sh --salvage-before-scrape --sync-gui --target NAME
```

**KotOR yes_general** (`221726893064454144`): first catch-up after a 2021 archive cursor can take hours and may OOM; salvage preserved partials before retrying. Stop duplicate validation processes (MyBook vs Downloads checkouts share the same lock). For large catch-up, set `DCE_CONTAINER_MEMORY=8g` in `scrape.env` (or export before the run), then:

```bash
./scripts/run-operator-validation.sh --salvage-before-scrape \
  --target KotOR_discord_msgs --channel 221726893064454144 \
  --log-file logs/kotor-yes-general.log
```

## GUI zip only

See [gui-zip-recurring-scrape-bridge.md](gui-zip-recurring-scrape-bridge.md), run `./scripts/sync-gui-bridge-doc.sh`, or use `../DiscordChatExporter.linux-x64/bootstrap-recurring-scrape.sh`.

Validate scripts after changes:

```bash
./scripts/run-all-smokes.sh
```

Merge / review summary: [recurring-scrape-merge-readiness.md](recurring-scrape-merge-readiness.md)

Full detail: [.docs/Recurring-Scrape-Setup.md](../.docs/Recurring-Scrape-Setup.md)
