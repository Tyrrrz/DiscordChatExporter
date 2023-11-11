# Docker usage instructions

Docker distribution of DiscordChatExporter provides a way to run the app in a virtualized and isolated environment. Due to the nature of Docker, you also don't need to install any prerequisites otherwise required by DCE.

> **Note**:
> Only the CLI flavor of DiscordChatExporter is available for use with Docker.

## Pulling

This will download the [Docker image from the registry](https://hub.docker.com/r/tyrrrz/discordchatexporter) to your computer. You can run this command again to update when a new version is released.

```console
docker pull tyrrrz/discordchatexporter:stable
```

Note the `:stable` tag. DiscordChatExporter images are tagged according to the following patterns:

- `stable` — latest stable version release. This tag is updated with each release of a new project version. Recommended for personal use.
- `x.y.z` (e.g. `2.30.1`) — specific stable version release. This tag is pushed when the corresponding version is released and never updated thereafter. Recommended for use in automation scenarios.
- `latest` — latest (potentially unstable) build. This tag is updated with each new commit to the `master` branch. Not recommended, unless you want to test a new feature that has not been released in a stable version yet.

You can see all available tags [here](https://hub.docker.com/r/tyrrrz/discordchatexporter/tags?ordering=name).

## Usage

To run the CLI in Docker and render help text:

```console
docker run --rm tyrrrz/discordchatexporter:stable
```

To export a channel:

```console
docker run --rm -v /path/on/machine:/out tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
```

If you want colored output and real-time progress reporting, pass the `-it` (interactive + pseudo-terminal) option:

```console
docker run --rm -it -v /path/on/machine:/out tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
```

The `-v /path/on/machine:/out` option instructs Docker to bind the `/out` directory inside the container to a path on your host machine. Replace `/path/on/machine` with the directory you want the files to be saved at.

> **Note**:
> If you are running SELinux, you will need to add the `:z` option after `/out`, e.g.:
>
> ```console
> docker run --rm -v /path/on/machine:/out:z tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
> ```
>
> For more information, refer to the [Docker docs SELinux labels for bind mounts page](https://docs.docker.com/storage/bind-mounts/#configure-the-selinux-label).

You can also use the current working directory as the output directory by specifying:

- `-v $PWD:/out` in Bash
- `-v $pwd.Path:/out` in PowerShell

For more information, please refer to the [Dockerfile](https://github.com/Tyrrrz/DiscordChatExporter/blob/master/DiscordChatExporter.Cli.dockerfile) and [Docker documentation](https://docs.docker.com/engine/reference/run).

To get your Token and Channel IDs, please refer to [this page](Token-and-IDs.md).

## Environment variables

DiscordChatExpoter CLI accepts the `DISCORD_TOKEN` environment variable as a fallback for the `--token` option. You can set this variable either with the `--env` Docker option or with a combination of the `--env-file` Docker option and a `.env` file.

Please refer to the [Docker documentation](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file) for more information.
