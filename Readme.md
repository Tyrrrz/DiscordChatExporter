# DiscordChatExporter

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/DiscordChatExporter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/DiscordChatExporter)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)

DiscordChatExporter can be used to export message history from a [Discord](https://discordapp.com) channel to a file. It works for both direct message chats and guild chats, supports markdown, message grouping, embeds, attachments, mentions, reactions and other features. It works with both user and bot tokens, supports multiple output formats and allows you to trim messages by dates.

## Download

- [Stable releases](https://github.com/Tyrrrz/DiscordChatExporter/releases)
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/DiscordChatExporter)
- [Docker](https://hub.docker.com/r/tyrrrz/discordchatexporter): `docker pull tyrrrz/discordchatexporter` (only CLI version)

## Features

- Graphical and command line interfaces
- Supports both user tokens and bot tokens
- Allows retrieving messages in specified date range
- Multiple export formats: HTML (dark/light), TXT and CSV
- Renders all message features including: markdown, attachments, embeds, emojis, mentions, etc

## Screenshots

![](http://www.tyrrrz.me/Projects/DiscordChatExporter/Images/1.png)
![](http://www.tyrrrz.me/Projects/DiscordChatExporter/Images/4.png)

## Libraries used

- [Stylet](https://github.com/canton7/Stylet)
- [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Newtonsoft.Json](http://www.newtonsoft.com/json)
- [Scriban](https://github.com/lunet-io/scriban)
- [CommandLineParser](https://github.com/commandlineparser/commandline)
- [Ookii.Dialogs](https://github.com/caioproiete/ookii-dialogs-wpf)
- [ByteSize](https://github.com/omar/ByteSize)
- [Failsafe](https://github.com/Tyrrrz/Failsafe)
- [Gress](https://github.com/Tyrrrz/Gress)
- [Onova](https://github.com/Tyrrrz/Onova)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.Settings](https://github.com/Tyrrrz/Settings)
