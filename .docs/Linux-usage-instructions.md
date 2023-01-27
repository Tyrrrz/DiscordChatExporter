# Linux usage instructions

### Step 1: Install .NET Core Runtime
Please follow the [instructions provided here](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Install-.NET-Core-runtime).

### Step 2: Downloading and using DiscordChatExporter.Cli
**1.** Download [DiscordChatExporter.CLI.zip](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) and extract it to a folder.<br/>
**2.** Open Terminal.<br/>
**3.** `cd` into the extracted folder. You can do this in Terminal by typing `cd`, then press the SPACE key, drag and drop the extracted folder into the Terminal window, and press the ENTER key.<br/>
**4.** Replace `TOKEN` and `CHANNEL`, then execute this command to export:
```
dotnet DiscordChatExporter.Cli.dll export -t TOKEN -c CHANNEL
```
> 💡 Tip: [How to get Token and Channel IDs](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs). 

There's much more you can do with DCE.CLI! Read the [CLI explained](https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs/GUI%2C-CLI-and-Formats-explained.md#dcecli-commands-) page to get started.