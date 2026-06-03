# Recurring Discord Scrape Automation - Setup Guide

This guide walks you through setting up automated recurring Discord exports using the built-in wrapper scripts.

## Prerequisites

- Linux or macOS with bash and cron
- Docker or Podman installed
- A Discord bot token or user token with access to the channels you want to export
- Read/write access to a directory for archive storage

## Quick Start

**Fastest path:** `./scripts/operator-handoff.sh` (disk + verify + archive dry-run), then `./scripts/bootstrap-recurring-scrape.sh` without `--dry-run` when ready (see [operator checklist](../docs/recurring-scrape-operator-checklist.md)).

**One-target live proof:** `./scripts/run-operator-proof.sh --sync-gui --target eod_discord` (handoff → scrape → grow-only check; use `--dry-run` for handoff only).

**Disk space:** `verify-operator-ready` and scrape entrypoints fail below 1 GiB free by default (`DCE_MIN_FREE_MB`, default 1024). Large channel JSON merges need extra headroom.

**Append-only contract (read first)**

- Each target writes under its configured `output_dir` (for example `~/Documents/KotOR_discord_msgs/`).
- Existing files named `Guild - Category - Channel [channel_id].json` are discovered automatically and updated in place.
- On the first run against an existing archive tree, the wrapper bootstraps `output_dir/.dce-meta/channel-map.json` from those filenames so it never creates a parallel export file.
- Incremental exports use DiscordChatExporter `--after` with the highest existing message id, then merge new messages by id.
- A merge that would reduce message count is rejected; the on-disk archive is left unchanged.

### 1. Configure Your Targets

Create or edit `config/scrape-targets.json` with your channel selections:

```json
{
  "archive_root": "/home/user/discord-archives",
  "defaults": {
    "include_threads": "all",
    "include_voice_channels": false
  },
  "targets": [
    {
      "name": "my-servers",
      "kind": "guild",
      "output_dir": "/home/user/discord-archives/my-servers",
      "guild_ids": ["123456789"],
      "channel_ids": [],
      "guild_name_patterns": []
    }
  ]
}
```

**Key fields:**
- `archive_root`: Parent directory for all exports (used for validation and path safety)
- `output_dir`: Specific directory for each target (must be under archive_root)
- `guild_ids`: Explicit Discord guild IDs (especially important for bot tokens)
- `channel_ids`: Specific channels to export (leave empty to export all accessible)
- `guild_name_patterns`: Regex patterns to match guild names (not used by bot tokens)

### 2. Set Your Discord Token

**Cron requires `scrape.env`.** Manual `export DISCORD_TOKEN` works for one-off runs, but scheduled jobs run in a minimal environment and need a persisted env file.

Either copy the environment template:

```bash
cp scrape.env.example scrape.env
# Edit scrape.env and set DISCORD_TOKEN=your-token-here
# OR set DISCORD_TOKEN_FILE=/path/to/token/file for automatic token rotation
```

Or export the token directly in your shell (the host wrapper accepts this when `scrape.env` is absent):

```bash
export DISCORD_TOKEN="your-token-here"
# optional: export DISCORD_TOKEN_FILE=/path/to/token/file
```

When no explicit token is set, the host wrapper runs `scripts/discover-discord-token.sh`, which tries (in order): `DISCORD_TOKEN` / `DISCORD_TOKEN_FILE`, optional `~/.config/discord-scrape/token`, DiscordChatExporter GUI `Settings.dat` (via `scripts/read-dce-gui-token.sh` when `DISCORDCHATEXPORTER_SETTINGS_PATH` or a sibling `Settings.dat` next to the CLI binary is present), then Discord desktop `leveldb` token candidates (longest match wins).

To materialize `scrape.env` from exported credentials (mode `600`, no manual editing):

```bash
export DISCORD_TOKEN="your-token-here"
./scripts/setup-scrape-auth.sh
```

### 2b. Verify existing ~/Documents archives

Before the first incremental run, confirm each enabled target points at the correct on-disk server folder and already has seeded channel JSON exports (the scraper appends in place and bootstraps `.dce-meta/channel-map.json` from these files):

```bash
./scripts/verify-documents-archives.sh --config config/scrape-targets.json
```

Each enabled target should show a non-zero **JSON** count and **SEEDED** channel IDs under your configured `output_dir` values (see `archive_root` in `config/scrape-targets.json`).

**One-command workflow** (verify → preflight → incremental scrape):

```bash
export DISCORD_TOKEN="your-token"   # or place token in ~/.config/discord-scrape/token
./scripts/run-documents-scrape.sh
./scripts/run-documents-scrape.sh --target KotOR_discord_msgs   # single server
./scripts/run-documents-scrape.sh --dry-run                     # archives only, no Discord
```

After a scrape, prove archives only grew in place:

```bash
./scripts/prove-incremental-append.sh --target KotOR_discord_msgs
```

### 3. Run Preflight Validation

Before installing cron, validate your setup:

```bash
export DISCORD_TOKEN="your-token"
./scripts/run-discord-scrape-host.sh preflight --config config/scrape-targets.json
```

Or run inside the container workflow directly:

```bash
./scripts/run-discord-scrape.sh preflight --config config/scrape-targets.json
```

This will:
- Check token validity
- Verify all configured targets are accessible
- Show which channels will be scraped
- Confirm archive directories are writable
- Make NO changes to archives or cron

### 4. Install the Cron Job

Once preflight passes, install the recurring export:

```bash
./scripts/setup-cron.sh --config config/scrape-targets.json
```

This creates a managed cron entry that runs monthly (default). The entry can be updated or removed later.

### 5. Verify Installation

Check that the cron job was installed:

```bash
crontab -l | grep discord-scrape
```

## Customizing the Schedule

The default is monthly. Customize it with:

```bash
# Run every day at 2 AM
./scripts/setup-cron.sh --config config/scrape-targets.json --interval "daily" --at "2:00"

# Run every Sunday at noon (weekly uses Sunday; time is HH:MM only)
./scripts/setup-cron.sh --config config/scrape-targets.json --interval weekly --at "12:00"

# Custom cron expression (every 6 hours)
./scripts/setup-cron.sh --config config/scrape-targets.json --cron "0 */6 * * *"
```

## Token Rotation

If using `DISCORD_TOKEN_FILE`, the host wrapper can automatically reload your token on each run:

```bash
# Protect your token file
chmod 600 /path/to/token/file

# Configure in scrape.env
DISCORD_TOKEN_FILE=/path/to/token/file
DCE_USERNS_MODE=keep-id  # for rootless podman
```

On each scheduled run, if the export fails with a `401` or `403` error, the wrapper:
1. Reloads the token file
2. Retries the export once
3. Logs the result

This keeps your token fresh without manual intervention.

## Archive Layout

After first export, your archive directory will contain:

```
archive_root/
├── .dce-meta/
│   ├── channel-map.json        # Channel ID to file mappings
│   └── locks/                  # Per-target locks (during active runs)
├── my-servers/
│   ├── .dce-meta/
│   │   └── channel-map.json
│   ├── Guild Name - Category - Channel [123456].json
│   ├── Another Guild - General [789012].json
│   └── ...
└── ...
```

Existing exports are updated in-place with new messages appended and deduplicated by message ID. See **Append-only contract** at the top of this guide.

## Troubleshooting

For common issues and solutions, see [Recurring-Scrape-Troubleshooting.md](Recurring-Scrape-Troubleshooting.md).

## Advanced Configuration

### Bot Tokens vs User Tokens

**Bot tokens** cannot enumerate guilds or DMs, so you must provide explicit IDs:
```json
{
  "name": "bot-scraped",
  "kind": "guild",
  "guild_ids": ["123456789", "987654321"],
  "channel_ids": ["111222333"]
}
```

**User tokens** can auto-discover but are against Discord TOS for automated use:
```json
{
  "name": "user-scraped",
  "kind": "guild",
  "guild_ids": [],  // Will auto-discover
  "channel_ids": []  // Will auto-discover
}
```

### Disabling Targets

Temporarily disable a target without removing it:

```json
{
  "name": "disabled-target",
  "enabled": false,
  "kind": "guild",
  ...
}
```

### SELinux and Rootless Podman

For SELinux:
```bash
# Label mounts for relabeling (already in docker-compose.yml)
DCE_MOUNT_OPTIONS=z
```

For rootless podman:
```bash
# Keep mounted dirs writable as your user
DCE_USERNS_MODE=keep-id
DCE_UID=$(id -u)
DCE_GID=$(id -g)
```

## Managing Cron

### View Current Schedule

```bash
crontab -l
```

### Update Schedule

Re-run setup with new parameters (old entry replaced):

```bash
./scripts/setup-cron.sh --config config/scrape-targets.json --interval "daily"
```

### Dry-run (Preview Changes)

```bash
./scripts/setup-cron.sh --config config/scrape-targets.json --dry-run
```

### Remove Cron Entry

```bash
./scripts/setup-cron.sh --remove
```

## Monitoring Exports

Check logs from your last run:

```bash
# Primary log file (default from setup-cron.sh)
tail -f logs/discord-scrape.log

# Recent cron execution (system log)
sudo grep discord-scrape /var/log/syslog  # Debian/Ubuntu
sudo grep discord-scrape /var/log/cron    # CentOS/RHEL

# Container build/run issues
docker compose logs -f
```

After a scheduled run, confirm archives grew in place:

```bash
./scripts/prove-incremental-append.sh --target KotOR_discord_msgs
```

## Performance Considerations

- **First export** of a channel can be slow (API rate-limited)
- **Incremental updates** are much faster (only new messages)
- **Large channels** (100k+ messages) may take several minutes
- **Rate limiting**: Discord's API has strict per-user limits; repeated failures may indicate you've hit them

Space requirements:
- **Typical channel**: 1-10 MB per year of messages
- **Large channels**: 50-100 MB per year
- **Full guild**: 500 MB - several GB depending on activity
- **Multi-year catch-up in container:** may OOM on first export; set `DCE_CONTAINER_MEMORY=8g` in `scrape.env` and use `--salvage-before-scrape` (see [Troubleshooting](Recurring-Scrape-Troubleshooting.md#channel-export-skipped-oom--aborted--killed))

## Smoke test validation

Run the full offline suite from the repo root (requires `jq`). **21 offline smokes** run by default; add `--include-container` for a 22nd local-only check:

```bash
./scripts/run-all-smokes.sh
```

With Docker/Podman, include the container smoke:

```bash
./scripts/run-all-smokes.sh --include-container
```

**Archive integrity helpers** (not smokes; run against live `~/Documents` trees):

```bash
./scripts/audit-archive-json.sh --target KotOR_discord_msgs
./scripts/salvage-truncated-export.sh path/to/export.json   # truncated JSON only
./scripts/prove-incremental-append.sh --target NAME        # live grow-only proof (needs token)
```

| Script | CI (`recurring-scrape-smoke`) | Notes |
|--------|-------------------------------|-------|
| `archive-disk-space-smoke.sh` | yes | Disk preflight / `DCE_MIN_FREE_MB` |
| `audit-archive-json-smoke.sh` | yes | Invalid JSON detection |
| `bootstrap-recurring-scrape-smoke.sh` | yes | Bootstrap dry-run |
| `cron-idempotency-smoke.sh` | yes | Cron installer idempotency |
| `documents-scrape-smoke.sh` | yes | Unified Documents workflow + lock gate |
| `end-to-end-preflight-smoke.sh` | yes | Preflight wiring |
| `error-path-smoke.sh` | yes | Failure paths |
| `gh-approve-pr-runs-smoke.sh` | yes | Fork PR workflow helper |
| `operator-handoff-smoke.sh` | yes | Operator handoff dry-run |
| `prove-incremental-append-smoke.sh` | yes | Offline prove snapshot/compare |
| `run-discord-scrape-host-lock-smoke.sh` | yes | Archive-root scrape lock |
| `run-discord-scrape-host-smoke.sh` | yes | Host wrapper |
| `run-discord-scrape-smoke.sh` | yes | Append-only merge coverage |
| `run-operator-proof-smoke.sh` | yes | Scrape + prove dry-run |
| `run-operator-validation-smoke.sh` | yes | Validation runner dry-run |
| `scrape-here-smoke.sh` | yes | Workspace bridge launcher |
| `scrape-lock-status-smoke.sh` | yes | Lock status + stale reclaim |
| `setup-cron-smoke.sh` | yes | Cron setup dry-run |
| `sync-gui-bridge-doc-smoke.sh` | yes | GUI bridge doc sync |
| `verify-documents-auth-smoke.sh` | yes | Archive verify + auth bootstrap |
| `verify-operator-ready-smoke.sh` | yes | Host prerequisite checks |
| `container-smoke.sh` | no (local) | Docker build + `help` / `list-targets`; use `--include-container` |

GitHub Actions runs `./scripts/run-all-smokes.sh` via `.github/workflows/main.yml` job `recurring-scrape-smoke`.

## Next Steps

- [Troubleshooting common issues](Recurring-Scrape-Troubleshooting.md)
- [Scheduling documentation for your OS](Scheduling-Linux.md)
- [Docker and containerization details](Docker.md)
