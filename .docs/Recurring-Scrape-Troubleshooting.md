# Recurring Discord Scrape Automation - Troubleshooting Guide

This guide covers common issues and their solutions.

## Setup Issues

### "Required file not found" Error

**Symptoms:** Setup fails with "Required file not found: /path/to/config.json"

**Solutions:**
1. Verify config file exists: `ls -la config/scrape-targets.json`
2. Check file permissions: `chmod 644 config/scrape-targets.json`
3. Use absolute path in setup command: `./scripts/setup-cron.sh --config $(pwd)/config/scrape-targets.json`

---

### "Invalid JSON config" Error

**Symptoms:** Setup fails with "Invalid JSON config: ..."

**Solutions:**
1. Validate JSON syntax: `jq empty config/scrape-targets.json`
2. Common mistakes:
   - Trailing commas in arrays/objects
   - Unquoted keys
   - Missing closing braces
3. Use an online JSON validator if needed

---

### "DISCORD_TOKEN must be set" Error

**Symptoms:** Preflight or scrape fails with token error

**Solutions:**
1. Set token in current session:
   ```bash
   export DISCORD_TOKEN="your-token-here"
   ./scripts/run-discord-scrape.sh preflight
   ```

2. Or set in scrape.env and source it:
   ```bash
   source scrape.env
   ./scripts/run-discord-scrape.sh preflight
   ```

3. Or use DISCORD_TOKEN_FILE for file-based tokens:
   ```bash
   export DISCORD_TOKEN_FILE="/path/to/token/file"
   chmod 600 /path/to/token/file
   ```

---

### "Target output_dir is outside archive_root" Error

**Symptoms:** Setup fails with path validation error

**Solution:** Update config to ensure output_dir is under archive_root:

```json
{
  "archive_root": "/home/user/discord-archives",
  "targets": [
    {
      "output_dir": "/home/user/discord-archives/target1"  // ✓ Under archive_root
    }
  ]
}
```

Not this:
```json
{
  "archive_root": "/home/user/discord-archives",
  "targets": [
    {
      "output_dir": "/tmp/exports"  // ✗ Outside archive_root
    }
  ]
}
```

---

## Authentication Issues

### "Guild discovery failed" Error

**Symptoms:** Preflight or scrape fails with guild discovery message

**Causes:**
- Using a bot token (cannot enumerate guilds)
- Invalid token
- Token lacks required permissions

**Solutions:**

1. **For bot tokens:** Provide explicit guild and channel IDs:
   ```json
   {
     "name": "my-target",
     "guild_ids": ["123456789"],
     "channel_ids": ["111222333"]
   }
   ```

2. **For user tokens:** Ensure the token is valid:
   - Generate a new token from Discord Developer Portal
   - Test token validity: `DISCORD_TOKEN=xxx ./scripts/run-discord-scrape.sh list-targets`

3. **Check permissions:**
   - Bot needs at least "Read Messages/View Channels" and "Read Message History"
   - User token needs access to the target guilds/channels

---

### "Export ... belongs to channel XXX, expected YYY" Error

**Symptoms:** Scrape fails when updating an existing archive

**Cause:** Archive's embedded channel ID doesn't match the configured channel

**Solutions:**

1. **Update config to match archive:**
   - Check the existing archive file for the correct channel ID
   - Update channel_ids in config

2. **Or move the archive:**
   ```bash
   mv archive/old-location.json archive/target1/
   ```

3. **Or update the channel mapping manually:**
   ```bash
   jq '.["111"] = "path/to/archive.json"' archive/.dce-meta/channel-map.json > tmp.json && mv tmp.json archive/.dce-meta/channel-map.json
   ```

---

## Cron Schedule Issues

### Cron Job Not Running

**Symptoms:** Cron job installed but exports aren't happening

**Diagnostic steps:**

1. Verify cron is installed:
   ```bash
   crontab -l | grep discord-scrape
   ```

2. Check if cron daemon is running:
   ```bash
   sudo systemctl status cron
   # or on macOS:
   sudo launchctl list | grep cron
   ```

3. Check system logs:
   ```bash
   # Linux
   sudo grep CRON /var/log/syslog
   # or
   sudo grep discord-scrape /var/log/cron
   
   # macOS
   log stream --predicate 'eventMessage contains[c] "cron"'
   ```

4. Test the script manually:
   ```bash
   source scrape.env
   bash scripts/run-discord-scrape-host.sh scrape
   ```

---

### "No such file or directory" in Cron Logs

**Symptoms:** Cron log shows script not found even though it exists

**Causes:**
- Path in crontab uses relative paths
- Directory changed since cron was installed
- Script permissions changed

**Solutions:**

1. Re-install cron with absolute paths:
   ```bash
   cd /path/to/DiscordChatExporter
   ./scripts/setup-cron.sh --config $(pwd)/config/scrape-targets.json
   ```

2. Ensure script is executable:
   ```bash
   chmod +x scripts/run-discord-scrape-host.sh
   chmod +x scripts/run-discord-scrape.sh
   chmod +x scripts/setup-cron.sh
   ```

---

### Cron Jobs Running at Wrong Time

**Symptoms:** Export runs at unexpected times

**Solutions:**

1. Check timezone setting:
   ```bash
   date  # System time
   timedatectl  # System timezone
   ```

2. Verify crontab schedule:
   ```bash
   crontab -l
   ```

3. Update schedule:
   ```bash
   ./scripts/setup-cron.sh --interval "daily" --at "2:00"
   ```

4. Validate cron expression at [crontab.guru](https://crontab.guru)

---

## Export Issues

### Exports Complete but Produce Empty Files

**Symptoms:** Archive files created but contain minimal/no messages

**Solutions:**

1. Verify channels are accessible:
   ```bash
   export DISCORD_TOKEN="your-token"
   ./scripts/run-discord-scrape.sh preflight
   ```

2. Check channel permissions:
   - Ensure token has "Read Message History"
   - Verify channel is not archived/deleted

3. Manual test export:
   ```bash
   ./scripts/run-discord-scrape.sh scrape --target target-name
   ```

---

### "Archive is not valid JSON" Error

**Symptoms:** Existing archive file becomes corrupted

**Solutions:**

1. **Audit all archives for a target:**
   ```bash
   ./scripts/audit-archive-json.sh --target target-name
   ```

2. **Validate one file:**
   ```bash
   jq empty archive-file.json
   ```

3. **Truncated export (parse error mid-message):** salvage drops the incomplete tail and keeps earlier messages. A timestamped `.bak.*` backup is created first:
   ```bash
   ./scripts/salvage-truncated-export.sh path/to/export.json
   ```

4. **If corrupted beyond salvage, restore from backup** (if available)

5. **If no backup, move the archive aside and re-export:**
   ```bash
   mv archive-file.json archive-file.json.bak
   ./scripts/run-discord-scrape.sh scrape --target target-name
   ```

---

### Incremental Exports Are Too Slow

**Symptoms:** Each scheduled export takes several minutes

**Solutions:**

1. **Check API rate limiting:**
   - Discord limits API calls per user
   - Too many frequent exports can trigger rate limiting
   - Increase interval between exports: `--interval "weekly"`

2. **Reduce scope:**
   - Export only recent messages: configure `after` date in export
   - Split large channels into separate targets

3. **Check system resources:**
   - Disk I/O bottleneck: `iostat -x 1`
   - CPU usage: `top`
   - Memory: `free -h`

---

### "Failed to write archive" or Permission Denied

**Symptoms:** Export fails with write permission errors

**Solutions:**

1. **Check directory permissions:**
   ```bash
   ls -la archive/target-name/
   chmod 755 archive/target-name/
   chmod 644 archive/target-name/*.json
   ```

2. **If using Docker/Podman, set user mode:**
   ```bash
   # For rootless podman
   export DCE_USERNS_MODE=keep-id
   export DCE_UID=$(id -u)
   export DCE_GID=$(id -g)
   ```

3. **Check SELinux (if enabled):**
   ```bash
   getenforce
   # If "Enforcing", add `:z` to mount options:
   # docker-compose.yml should already have this
   ```

---

## Docker/Container Issues

### "Failed to build image" Error

**Symptoms:** Docker build fails during setup

**Solutions:**

1. **Verify Docker is running:**
   ```bash
   docker ps
   docker version
   ```

2. **Check disk space:**
   ```bash
   docker system df
   ```

3. **Clean up and retry:**
   ```bash
   docker system prune -a
   docker-compose build --no-cache
   ```

4. **If using Podman:**
   ```bash
   podman system prune -a
   podman-compose build --no-cache
   ```

---

### "Cannot connect to Docker daemon" Error

**Symptoms:** Setup fails to reach Docker

**Solutions:**

1. **For Docker:**
   ```bash
   sudo systemctl start docker
   sudo usermod -aG docker $USER
   newgrp docker
   ```

2. **For Podman (rootless):**
   ```bash
   systemctl --user start podman
   systemctl --user enable podman
   ```

---

## Authorization / Token Refresh Issues

### Host Retry Auth Flow Not Working

**Symptoms:** Export fails with 401/403 errors even with DISCORD_TOKEN_FILE set

**Solutions:**

1. **Verify token file is readable:**
   ```bash
   cat $DISCORD_TOKEN_FILE
   ```

2. **Ensure proper permissions:**
   ```bash
   chmod 600 $DISCORD_TOKEN_FILE
   ```

3. **Check token is fresh:**
   - Tokens can expire
   - Generate a new token from Discord Developer Portal
   - Update the token file

4. **Verify host wrapper is being used:**
   ```bash
   grep run-discord-scrape-host scripts/run-discord-scrape-host.sh
   ```

---

## Getting Help

If you're still stuck:

1. **Check existing issues:** https://github.com/Tyrrrz/DiscordChatExporter/issues
2. **Run preflight in verbose mode:**
   ```bash
   set -x  # Enable debug output
   ./scripts/run-discord-scrape.sh preflight
   ```

3. **Check logs:**
   ```bash
   # Docker logs
   docker-compose logs --tail 50
   
   # Cron logs (on Linux)
   sudo journalctl -u cron --since "1 hour ago"
   ```

4. **Collect error details** for reporting issues:
   - Config (sanitize token)
   - Full error message
   - OS/Docker version
   - Steps to reproduce
