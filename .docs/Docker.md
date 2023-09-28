# Docker usage instructions

Docker distribution of DiscordChatExporter provides a way to run the app in a virtualized and isolated environment. Due to the nature of Docker, you also don't need to install any prerequisites otherwise required by DCE.

Note that only the CLI flavor of DiscordChatExporter is available for use with Docker.

## Pulling

This will download the [Docker image from the registry](https://hub.docker.com/r/tyrrrz/discordchatexporter) to your computer. You can run this command again to update when a new version is released.

```console
docker pull tyrrrz/discordchatexporter:stable
```

Note the `:stable` tag. DiscordChatExporter images are tagged according to the following patterns:

- `stable`: Latest stable version release and maps to the latest GitHub release. This tag is updated with release of each new version. Recommended for personal use.
- `latest`: Built from the latest commit and maps to the latest CI build on GitHub. This tag is updated with each new commit to `master` branch. Not recommended for everyday use, but you can use it if you want to try out some features that haven't been officially released yet.
- `x.y.z` (e.g. `2.30.1`): Fixed version release and maps to the corresponding tag on GitHub. This tag is never updated once that version has been released. Recommended for use in automation scenarios.

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

If you want colored output and real-time progress reporting, pass the `-it` (interactive) option:

```console
docker run --rm -it -v /path/on/machine:/out tyrrrz/discordchatexporter:stable export -t TOKEN -c CHANNELID
```

Note the `-v /path/on/machine:/out` option, which instructs Docker to bind the `/out` directory inside the container to a path on your host machine. Replace `/path/on/machine` with the directory you want the files to be saved at.

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

To get your Token and Channel IDs, please refer to [this page](https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs/Token-and-IDs.md).

## Environment variables

DiscordChatExpoter CLI accepts the `DISCORD_TOKEN` environment variable as a fallback for the `--token` option. You can set this variable either with the `--env` Docker option or with a combination of the `--env-file` Docker option and a `.env` file.

Please refer to the [Docker documentation](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file) for more information.

## Permission issues with Linux hosts

When bounding volumes between the container and the host, files mirrored from the Docker container will be owned by the Docker user (in most setups this is `root`). Please refer to this [issue comment](https://github.com/Tyrrrz/DiscordChatExporter/issues/800#issuecomment-1030471970) for instructions on how to override this behavior.
