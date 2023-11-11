# Linux usage instructions

## Installing .NET Runtime

Please follow the [instructions provided here](Dotnet.md).

## Downloading and using DiscordChatExporter.Cli

1. Download [DiscordChatExporter.CLI.zip](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) and extract it to a folder.
2. Open Terminal.
3. `cd` into the extracted folder. You can do this in Terminal by typing `cd`, then press the SPACE key, drag and drop the extracted folder into the Terminal window, and press the ENTER key.
4. Replace `TOKEN` and `CHANNEL`, then execute this command to export:

```console
dotnet DiscordChatExporter.Cli.dll export -t TOKEN -c CHANNEL
```

If the above command throws a "Permission denied" error, execute this command to fix the permissions:

```console
chmod 644 *.dll DiscordChatExporter.*
```

> [How to get Token and Channel IDs](Token-and-IDs.md).

There's much more you can do with DCE.CLI! Read the [CLI explained](Using-the-CLI.md) page to get started.
