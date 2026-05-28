# Docker usage instructions

Docker distribution of DiscordChatExporter provides a way to run the app in a virtualized and isolated environment. Due to the nature of Docker, you also don't need to install any prerequisites otherwise required by DCE.

> **Note**:
> Only the CLI flavor of DiscordChatExporter is available for use with Docker.

## Pulling

This will download the [Docker image from the registry](https://hub.docker.com/r/tyrrrz/discordchatexporter) to your computer. You can run this command again to update when a new version is released.

```console
$ docker pull tyrrrz/discordchatexporter:stable
```

Note the `:stable` tag. DiscordChatExporter images are tagged according to the following patterns:

- `stable` — latest stable version release. This tag is updated with each release of a new project version. Recommended for personal use.
- `x.y.z` (e.g. `2.30.1`) — specific stable version release. This tag is pushed when the corresponding version is released and never updated thereafter. Recommended for use in automation scenarios.
- `latest` — latest (potentially unstable) build. This tag is updated with each new commit to the `prime` branch. Not recommended, unless you want to test a new feature that has not been released in a stable version yet.

You can see all available tags [here](https://hub.docker.com/r/tyrrrz/discordchatexporter/tags?ordering=name).

## Usage

To run the CLI in Docker and render help text:

```console
$ docker run --rm tyrrrz/discordchatexporter:stable
```

To export a channel:

```console
$ docker run --rm -v /path/on/machine:/out tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
```

If you want colored output and real-time progress reporting, pass the `-it` (interactive + pseudo-terminal) option:

```console
$ docker run --rm -it -v /path/on/machine:/out tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
```

The `-v /path/on/machine:/out` option instructs Docker to bind the `/out` directory inside the container to a path on your host machine. Replace `/path/on/machine` with the directory you want the files to be saved at.

> **Note**:
> If you are running SELinux, you will need to add the `:z` option after `/out`, e.g.:
>
> ```console
> $ docker run --rm -v /path/on/machine:/out:z tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
> ```
>
> For more information, refer to the [Docker docs SELinux labels for bind mounts page](https://docs.docker.com/storage/bind-mounts/#configure-the-selinux-label).

You can also use the current working directory as the output directory by specifying:

- `-v $PWD:/out` in Bash
- `-v $pwd.Path:/out` in PowerShell

For more information, please refer to the [Dockerfile](https://github.com/Tyrrrz/DiscordChatExporter/blob/prime/DiscordChatExporter.Cli.dockerfile) and [Docker documentation](https://docs.docker.com/engine/reference/run).

To get your Token and Channel IDs, please refer to [this page](Token-and-IDs.md).

## Unix permissions issues

This image was designed with a user running as uid:gid of 1000:1000.

If your current user has different IDs, and you want to generate files directly editable for your user, you might want to run the container like this:

```console
$ mkdir data # or chown -R $(id -u):$(id -g) data
$ docker run -it --rm -v $PWD/data:/out --user $(id -u):$(id -g) tyrrrz/discordchatexporter:stable export -t TOKEN -g CHANNELID
```

## Environment variables

DiscordChatExpoter CLI accepts the `DISCORD_TOKEN` environment variable as a fallback for the `--token` option. You can set this variable either with the `--env` Docker option or with a combination of the `--env-file` Docker option and a `.env` file.

Please refer to the [Docker documentation](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file) for more information.

## Source-built recurring scraper

This repo also ships a local recurring wrapper around the CLI for source-built automation:

- `Dockerfile` builds `DiscordChatExporter.Cli` from source.
- `docker-compose.yml` runs the wrapper container.
- `scripts/run-discord-scrape-host.sh` is the host-side entrypoint for scheduled runs.
- `scripts/run-discord-scrape.sh preflight` validates token/config/target resolution without writing archives.
- `scripts/run-discord-scrape.sh scrape` performs append-oriented JSON updates by exporting newer messages and merging them into the existing local archive instead of blindly replacing the destination file.

For the recurring flow, keep secrets in `scrape.env` (copied from `scrape.env.example`) and keep target/output mapping in `config/scrape-targets.json`. You can provide a direct `DISCORD_TOKEN` or a `DISCORD_TOKEN_FILE` path whose first line is the token.

For recurring runs, targets with `enabled: false` are skipped by default. This is the recommended way to keep unresolved archive roots in the config without blocking the rest of the schedule.

If you authenticate with a **bot token**, do not rely on guild-name or DM discovery. Discord does not expose "list my accessible guilds" or DM enumeration for bot-only REST auth, so recurring targets should be configured with explicit `guild_ids` / `channel_ids` or backed by existing archive files whose names already embed channel IDs (for example, `general [123456789012345678].json`). The `discord_dms` target should stay disabled unless you switch to a user token.

`preflight` now probes one resolved channel per selected target with the source-built CLI before cron is installed. If the token cannot read that channel, setup fails closed and leaves the existing crontab untouched.

The host wrapper (`scripts/run-discord-scrape-host.sh`) classifies Discord auth failures and retries once after reloading `DISCORD_TOKEN_FILE` (if configured). Persistent auth failure still exits non-zero.

For fork PRs blocked on GitHub Actions approval, repository admins can run `./scripts/gh-approve-pr-runs.sh --repo Tyrrrz/DiscordChatExporter RUN_ID` after exporting `GITHUB_TOKEN`. The script bootstraps `gh` auth and surfaces admin-rights policy blockers separately from token failures.

If you run the recurring flow through podman on an SELinux-enabled host, keep the bind mounts relabeled (`:z`). The checked-in `docker-compose.yml` already applies this to the recurring wrapper mounts.

For rootless podman, set `DCE_USERNS_MODE=keep-id` in `scrape.env` so the mounted archive roots stay writable as your host user instead of appearing as `root:root` inside the container. Keep `DCE_UID` and `DCE_GID` matched to your host user as well.

When the recurring wrapper sees an existing archive file whose name already embeds the channel ID (for example, `Deadly Stream - kotor-general [770102657795751976].json`), future runs keep updating that same path. When a channel is new and has no stored mapping yet, the wrapper now defaults to a human-readable filename based on the export metadata (`Guild - Category - Channel [id].json`) inside the configured target root instead of writing to `channels/<id>.json`.
