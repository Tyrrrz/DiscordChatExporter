# DiscordChatExporter

Command line executable that can export [Discord](https://discordapp.com) channel chat logs to a pretty HTML file.

## Screenshots

![](http://www.tyrrrz.me/projects/images/discordchatexporter_1.png)

## Features

- Produces output styled similar to the Discord's light theme
- Displays user avatars
- Groups messages by author and time
- Handles Discord markdown characters
- Converts URLs to links
- Inlines attached images with a link to the full version
- Inserts download links for other types of attached files
- Marks edited messages with a timestamp

## Usage

The program expects an access token and channel ID as parameters.

`DiscordChatExporter.exe /token:REkOTVqm9RWOTNOLCdiuMpWd.QiglBz.Lub0E0TZ1xX4ZxCtnwtpBhWt3v1 /channelId:459360869055190534`

### Getting access token:

- Open Discord desktop or web client
- Press `Ctrl+Shift+I`
- Navigate to `Storage > LocalStorage > discordapp.com`
- Find the value for `token` and extract it

### Getting channel ID:

- Open Discord desktop or web client
- Navigate to any DM or server channel
- Extract the current URL:
    - If using desktop client, press `Ctrl+Shift+I`, type `window.location.href` in console and extract the result
    - If using web client, just take the current URL from the address bar
- Pull the ID from the URL:
    - If it's a DM channel, the format looks like this: `https://discordapp.com/channels/@me/CHANNEL_ID`
    - If it's a server channel, the format looks like this:
    `https://discordapp.com/channels/WHATEVER/CHANNEL_ID`

## Libraries used

- [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
