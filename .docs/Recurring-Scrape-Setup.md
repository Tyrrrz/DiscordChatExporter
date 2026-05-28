# Recurring Discord Scrape Automation - Setup Guide

This guide walks you through setting up automated recurring Discord exports using the built-in wrapper scripts.

## Prerequisites

- Linux or macOS with bash and cron
- Docker or Podman installed
- A Discord bot token or user token with access to the channels you want to export
- Read/write access to a directory for archive storage

## Quick Start

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

Each enabled target should show a non-zero **JSON** count and **SEEDED** channel IDs under `/home/brunner56/Documents/<server>/`.

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

# Run every Sunday at noon
./scripts/setup-cron.sh --config config/scrape-targets.json --interval "weekly" --at "sun 12:00"

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

Existing exports are updated in-place with new messages appended and deduplicated by message ID.

**In-place append contract**

- Each target writes under its configured `output_dir` (for example `~/Documents/KotOR_discord_msgs/`).
- Existing files named `Guild - Category - Channel [channel_id].json` are discovered automatically and updated in place.
- On the first run against an existing archive tree, the wrapper bootstraps `output_dir/.dce-meta/channel-map.json` from those filenames so it never creates a parallel export file.
- Incremental exports use DiscordChatExporter `--after` with the highest existing message id, then merge new messages by id.
- A merge that would reduce message count is rejected; the on-disk archive is left unchanged.

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
# Recent cron execution
sudo grep discord-scrape /var/log/syslog  # Debian/Ubuntu
sudo grep discord-scrape /var/log/cron    # CentOS/RHEL

# Or check via Docker logs if using containers
docker-compose logs -f
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

## Next Steps

- [Troubleshooting common issues](Recurring-Scrape-Troubleshooting.md)
- [Scheduling documentation for your OS](.docs/Scheduling-Linux.md)
- [Docker and containerization details](.docs/Docker.md)
