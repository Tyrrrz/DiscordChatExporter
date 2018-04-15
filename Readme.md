# DiscordChatExporter

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/DiscordChatExporter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/DiscordChatExporter)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)

DiscordChatExporter can be used to export message history from a [Discord](https://discordapp.com) channel to a file. It works for both direct message chats and guild chats, supports markdown, message grouping, and attachments. The tool also lets you select from/to dates to limit the exported messages. There are options to configure the output, such as date format, color theme, message grouping limit, etc.

## Screenshots

![](http://www.tyrrrz.me/Projects/DiscordChatExporter/Images/1.png)
![](http://www.tyrrrz.me/Projects/DiscordChatExporter/Images/4.png)

## Download

- [Stable releases](https://github.com/Tyrrrz/DiscordChatExporter/releases)
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/DiscordChatExporter)

## Features

- Intuitive GUI that displays available guilds and channels
- CLI as additional alternative to GUI
- Date ranges to limit messages
- Groups messages by author and time
- Exports to a plain text file
- Exports to an HTML file
  - Dark and light themes
  - User avatars
  - Inline image attachments
  - Full markdown support
  - Automatic links
  - Styled similarly to the app
- Renders custom emojis
- Resolves user, role and channel mentions

## Libraries used

- [GalaSoft.MVVMLight](http://www.mvvmlight.net)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Newtonsoft.Json](http://www.newtonsoft.com/json)
- [Onova](https://github.com/Tyrrrz/Onova)
- [FluentCommandLineParser](https://github.com/fclp/fluent-command-line-parser)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)
- [Tyrrrz.WpfExtensions](https://github.com/Tyrrrz/WpfExtensions)
- [Tyrrrz.Settings](https://github.com/Tyrrrz/Settings)
