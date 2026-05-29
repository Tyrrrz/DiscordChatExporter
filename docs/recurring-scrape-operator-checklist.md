# Recurring scrape operator checklist

Use this after cloning or opening the **source** repo (`DiscordChatExporter`, not the GUI zip alone).

## One-time setup

0. `./scripts/verify-operator-ready.sh` — jq, compose, auth, and archive folders.
1. `cp scrape.env.example scrape.env` and set `DISCORD_TOKEN`, or `./scripts/sync-token-from-gui.sh --force` (reads GUI `Settings.dat`).
2. `./scripts/bootstrap-recurring-scrape.sh --dry-run` — confirm every **enabled** target has seeded JSON under `output_dir`.
3. `./scripts/bootstrap-recurring-scrape.sh` — verify archives, build image, preflight Discord.
4. `./scripts/run-documents-scrape.sh` — first incremental append-only scrape.
5. `./scripts/prove-incremental-append.sh --target <name>` — optional grow-only proof.
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
./scripts/setup-cron.sh --target KotOR_discord_msgs --channel CHANNEL_ID
```

## GUI zip only

See `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` or run `../DiscordChatExporter.linux-x64/bootstrap-recurring-scrape.sh`.

Validate scripts after changes:

```bash
./scripts/run-all-smokes.sh
```

Merge / review summary: [recurring-scrape-merge-readiness.md](recurring-scrape-merge-readiness.md)

Full detail: [.docs/Recurring-Scrape-Setup.md](../.docs/Recurring-Scrape-Setup.md)
