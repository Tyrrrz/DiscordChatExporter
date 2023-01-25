![Might look different depending on your macOS version](https://i.imgur.com/gA8jFjJ.png)

### Step 1: Install .NET Core Runtime
Please follow the [instructions provided here](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Install-.NET-Core-runtime).

### Step 2: Downloading and using DiscordChatExporter.Cli
**1.** Download [DiscordChatExporter.CLI.zip](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) and extract it to a folder.<br/>
**2.** Search for `Terminal.app` in Spotlight (⌘+SPACE), then open it.<br/>
**3.** In the Terminal window, type `cd` , press the SPACE key, then drag and drop the extracted folder into the window, then press the RETURN key.<br/>
**4.** Execute the following command to export, replacing `TOKEN` and `CHANNEL` with your own values:

    dotnet DiscordChatExporter.Cli.dll export -t TOKEN -c CHANNEL

> 💡 Tip: [How to get Token and Channel IDs](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs). 

There's much more you can do with DCE.CLI! Read the [CLI explained](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#dcecli-commands-) page to get started.