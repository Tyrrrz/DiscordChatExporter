# DiscordChatExporter

Command line executable that can export [Discord](https://discordapp.com) channel chat logs to a pretty HTML file.

## Usage

Executing the application requires an access token and channel ID as parameters.

You can get your token by opening the Discord app, pressing `Ctrl+Shift+I`, navigating to `Storage > LocalStorage > discordapp.com` and extracting `token`'s value.

You can get the channel ID by enabling `Developer Mode` in `Settings > Appearance` and then right clicking on the channel and clicking on `Copy ID`.

- `DiscordChatExporter.exe -t REkOTVqm9RWOTNOLCdiuMpWd.QiglBz.Lub0E0TZ1xX4ZxCtnwtpBhWt3v1 -c 459360869055190534`

## Libraries used

- [CommandLineParser](https://github.com/gsscoder/commandline)
- [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
