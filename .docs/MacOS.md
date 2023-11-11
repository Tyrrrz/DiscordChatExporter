# macOS usage instructions

![Might look different depending on your macOS version](https://i.imgur.com/gA8jFjJ.png)

## Installing .NET Runtime

Please follow the [instructions provided here](Dotnet.md).

## Downloading and using DiscordChatExporter.Cli

1. Download [DiscordChatExporter.CLI.zip](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) and extract it to a folder.
2. Search for `Terminal.app` in Spotlight (⌘+SPACE), then open it.
3. In the Terminal window, type `cd` , press the SPACE key, then drag and drop the extracted folder into the window, then press the RETURN key.
4. Execute the following command to export, replacing `TOKEN` and `CHANNEL` with your own values:

```console
dotnet DiscordChatExporter.Cli.dll export -t TOKEN -c CHANNEL
```

> [How to get Token and Channel IDs](Token-and-IDs.md).

There's much more you can do with DCE.CLI! Read the [CLI explained](Using-the-CLI.md) page to get started.
