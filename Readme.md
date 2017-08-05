# DiscordChatExporter

Command line executable that can export [Discord](https://discordapp.com) channel chat logs to a pretty HTML file.

## Screenshots

![](http://www.tyrrrz.me/projects/images/discordchatexporter_1.png)

## Features

- Produced HTML is styled similar to the Discord's light theme
- Displays user avatars
- Messages are grouped by author and time
- Discord's markdown characters are correctly formatted
- URLs are automatically converted to links
- Attached images are inlined with a link to the full version
- Other attached files are rendered as a download link
- Edited messages are properly marked with the edit timestamp

## Usage

Executing the application requires an access token and channel ID as parameters.

You can get your token by opening the Discord app, pressing `Ctrl+Shift+I`, navigating to `Storage > LocalStorage > discordapp.com` and extracting `token`'s value.

You can get the channel ID by enabling `Developer Mode` in `Settings > Appearance` and then right clicking on the channel and clicking on `Copy ID`.

- `DiscordChatExporter.exe /token:REkOTVqm9RWOTNOLCdiuMpWd.QiglBz.Lub0E0TZ1xX4ZxCtnwtpBhWt3v1 /channelId:459360869055190534`

## Libraries used

- [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
