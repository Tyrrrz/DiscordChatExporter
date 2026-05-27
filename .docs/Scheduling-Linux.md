# Scheduling exports with Cron

## Preflight Validation

Before installing the cron job, you can validate that your token, configuration, and target setup are correct by running the preflight command:

```bash
# Set your token and run preflight
export DISCORD_TOKEN="your-token-here"
./scripts/run-discord-scrape.sh preflight --config config/scrape-targets.json
```

The preflight command:
- Verifies your Discord token is valid and authenticated
- Checks that all configured targets are accessible
- Validates that the archive root and output directories are writable
- Resolves configured channel/guild IDs without writing any archives
- Reports which channels will be scraped and their expected output locations

The preflight command is **read-only**: it does not modify archives, create crontab entries, or make any state changes. This makes it safe to run with your real token and existing archives before committing to automated cron runs.

If any preflight check fails, the setup will stop before installing crontab entries. This ensures that cron will not be configured in a broken state.

### Common Preflight Errors

**"Export ... belongs to channel 'XXX', expected 'YYY'"**
- The archive you're trying to update contains metadata for a different channel than the one you configured
- Solution: Verify the channel ID in your config matches the archive's embedded channel ID, or move the archive to a different target

**"Guild discovery failed"**
- You're using a bot token and need to provide explicit guild/channel IDs
- Solution: Add `guild_ids` and `channel_ids` to your target configuration

**"Output dir ... is outside archive_root"**
- A configured output directory is not within the configured archive root
- Solution: Update your `output_dir` to be under the `archive_root` in your config

---

## Recommended recurring wrapper

This repo now includes a source-built recurring wrapper around the CLI:

- `scripts/setup-cron.sh` installs, previews, updates, and removes one managed cron block.
- `scripts/run-discord-scrape-host.sh` runs preflight/scrape through compose and handles one auth-refresh retry.
- `Dockerfile` + `docker-compose.yml` build and run the CLI from source.
- `scripts/run-discord-scrape.sh preflight` validates token/config/target resolution without writing archives.
- `scripts/run-discord-scrape.sh scrape` performs append-oriented JSON updates so existing local history is retained instead of overwritten.

The recommended Linux flow is:

1. Copy `scrape.env.example` to `scrape.env` and set `DISCORD_TOKEN` (or set `DISCORD_TOKEN_FILE` for file-based token rotation).
2. Review `config/scrape-targets.json` and keep archive roots under the configured `archive_root`.
3. Run `./scripts/setup-cron.sh` for the default monthly schedule, or pass `--interval`, `--at`, or `--cron` to customize it.
4. Re-run the same script later to update the managed cron block idempotently. Use `--remove` to delete only the managed block.

The host cron schedule is authoritative for execution time. Container `TZ` only affects process/runtime timestamps.

Targets with `enabled: false` are skipped by default. Use that field for archive roots that you want to keep in the config but cannot currently resolve safely.

If you are using a **bot token**, do not depend on guild-name or DM discovery. Bot tokens cannot enumerate accessible guilds or direct messages through the Discord REST API, so recurring targets need either explicit `guild_ids` / `channel_ids` or existing archive filenames that already encode channel IDs. The recurring wrapper can seed channel selection from those archive filenames, but setup still probes one real channel per target before touching crontab state.

If any selected target fails that authenticated probe, `setup-cron.sh` stops without mutating the live crontab. In practice this means the token must already have access to every enabled target you expect cron to update.

For recurring runs, `setup-cron.sh` now installs a cron command that executes `scripts/run-discord-scrape-host.sh scrape ...`. The host wrapper retries once when it detects Discord auth failures (`401`/`403`) by reloading `DISCORD_TOKEN_FILE` if configured. This keeps cron non-interactive and fail-closed.

If you are running the recurring wrapper through podman on an SELinux-enabled host, keep the bind mounts relabeled (`:z`). The checked-in `docker-compose.yml` already includes that for the recurring config and archive mounts.

For rootless podman, set `DCE_USERNS_MODE=keep-id` in `scrape.env` so the mounted `Documents` archive roots stay writable as your host user during scheduled runs. Keep `DCE_UID` and `DCE_GID` matched to your host user as well.

Existing archive files remain the source of truth for recurring updates. If a channel already has a local JSON export whose filename embeds the channel ID, the wrapper updates that exact file in place. If the channel is new and has no stored mapping yet, the first export now lands in the configured target root with a human-readable default filename (`Guild - Category - Channel [id].json`) instead of `channels/<id>.json`.

## Creating the script

1. Open Terminal and create a new text file with `nano /path/to/DiscordChatExporter/cron.sh`

> **Note**:
> You can't use your mouse in nano, use the arrow keys to control the cursor (caret).

2. Paste the following into the text file:

```bash
#!/bin/bash
# Info: https://github.com/Tyrrrz/DiscordChatExporter/blob/prime/.docs

TOKEN=tokenhere
CHANNELID=channelhere
DLLFOLDER=dceFOLDERpathhere
FILENAME=filenamehere
EXPORTDIRECTORY=dirhere
EXPORTFORMAT=formathere
# Available export formats: plaintext, htmldark, htmllight, json, csv
# /\ CaSe-SeNsItIvE /\
# You can edit the export command on line 40 if you'd like to include more options like date ranges and date format. You can't use partitioning (-p) with this script.

# This will verify if EXPORTFORMAT is valid and will set the final file extension according to it. If the format is invalid, the script will display a message and exit.
if [[ "$EXPORTFORMAT" == "plaintext" ]]; then
FORMATEXT=.txt
elif [[ "$EXPORTFORMAT" == "htmldark" ]] || [[ "$EXPORTFORMAT" == "htmllight" ]]; then
FORMATEXT=.html
elif [[ "$EXPORTFORMAT" == "json" ]]; then
FORMATEXT=.json
elif [[ "$EXPORTFORMAT" == "csv" ]]; then
FORMATEXT=.csv
else
echo "$EXPORTFORMAT - Unknown export format"
echo "Available export formats: plaintext, htmldark, htmllight, csv, json"
echo "/\ CaSe-SeNsItIvE /\\"
exit 1
fi

# This will change the script's directory to DLLPATH, if unable to do so, the script will exit.
cd $DLLFOLDER || exit 1

# This will export your chat
./DiscordChatExporter.Cli export -t $TOKEN -c $CHANNELID -f $EXPORTFORMAT -o $FILENAME.tmp

# This sets the current time to a variable
CURRENTTIME=$(date +"%Y-%m-%d-%H-%M-%S")

# This will move the .tmp file to the desired export location, if unable to do so, it will attempt to delete the .tmp file.
if ! mv "$FILENAME.tmp" "${EXPORTDIRECTORY//\"}/$FILENAME-$CURRENTTIME$FORMATEXT" ; then
echo "Unable to move $FILENAME.tmp to $EXPORTDIRECTORY/$FILENAME-$CURRENTTIME$FORMATEXT."
echo "Cleaning up..."
  if ! rm -Rf "$FILENAME.tmp" ; then
  echo "Unable to remove $FILENAME.tmp."
  fi
exit 1
fi
exit 0
```

3. Replace:

- `tokenhere` with your [Token](Token-and-IDs.md).
- `channelhere` with a [Channel ID](Token-and-IDs.md).
- `dceFOLDERpathhere` with DCE's **directory path** (e.g. `/path/to/folder`, NOT `/path/to/folder/DiscordChatExporter.dll`).
- `filenamehere` with the exported channel's filename, without spaces.
- `dirhere` with the export directory (e.g. /home/user/Documents/Discord\ Exports).
- `formathere` with one of the available export formats.

> **Note**:
> Remember to escape spaces (add `\` before them) or to quote (") the paths (`"/home/my user"`)!

> **Note**:
> To save, hold down CTRL and then press O, if asked for a filename, type it and press ENTER. Hit CTRL+X to exit the text editor.
> [Check out this page](https://wiki.gentoo.org/wiki/Nano/Basics_Guide) if you want to know more about nano.

4. Make your script executable with `chmod +x /path/to/DiscordChatExporter/cron.sh`

5. Let's edit the cron file. If you want to run the script with your user privileges, edit it by running `crontab -e`. If you want to run the script as root, edit it with `sudo crontab -e`. If this is your first time running this command, you might be asked to select a text editor. Nano is easier for beginners.

6. Add the following to the end of the file `* * * * * /path/to/DiscordChatExporter/cron.sh >/tmp/discordchatexporter.log 2>/tmp/discordchatexportererror.log`. Don't forget to replace the `/path/to/DiscordChatExporter/cron.sh`!

> **Note**:
> If you don't want logs to be created, replace both `/tmp/discordchatexporter.log` with `/dev/null`.

Then replace the \*s according to:

![](https://i.imgur.com/RY7USM6.png)

---

**Examples**:

- If you want to execute the script at minute 15 of every hour: `15 * * * *`
- Every 30 minutes `*/30 * * * *`
- Every day at midnight `0 0 * * *`
- Every day at noon `0 12 * * *`
- Every day at 3, 4 and 6 PM `0 15,16,18 * * *`
- Every Wednesday at 9 AM `0 9 * * 3`

Verify your cron time [here](https://crontab.guru).

---

**Additional information**

The week starts on Sunday. 0 = SUN, 1 = MON ... 7 = SUN.

Be aware that if you set the day to '31', the script will only run on months that have the 31st day.

> [Learn more about running a cron job on the last day of the month here](https://stackoverflow.com/questions/6139189/cron-job-to-run-on-the-last-day-of-the-month) (expert).

The default filename for the exported channel is `YYYY-MM-DD-hh-mm-ss-yourfilename`. You can change it if you'd like.

Don't forget to update your token in the script after it has been reset!

---

Special thanks to [@Yudi](https://github.com/Yudi)
