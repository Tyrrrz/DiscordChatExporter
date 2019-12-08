# DiscordChatExporter

[![Build](https://github.com/Tyrrrz/DiscordChatExporter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/DiscordChatExporter/actions)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

DiscordChatExporter can be used to export message history from a [Discord](https://discordapp.com) channel to a file. It works for both direct message chats and guild chats, supports markdown, message grouping, embeds, attachments, mentions, reactions and other features. It works with both user and bot tokens, supports multiple output formats and allows you to trim messages by dates.

_For guides and other info -- check out the [wiki](https://github.com/Tyrrrz/DiscordChatExporter/wiki)._

## Download

- **[Latest release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest)**
- [Docker image](https://hub.docker.com/r/tyrrrz/discordchatexporter): `docker pull tyrrrz/discordchatexporter`
- [CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions)

Note: This application requires .NET Core runtime in order to run:

- .NET Core runtime for Windows [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x64-installer) | [x86](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x86-installer)
- .NET Core runtime for macOS (for CLI version only) [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-3.1.0-macos-x64-installer)
- .NET Core runtime for Linux (for CLI version only) [Ubuntu/Debian](https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1904#install-the-net-core-runtime)

## Features

- Graphical and command line interfaces
- Supports both user tokens and bot tokens
- Allows retrieving messages in specified date range
- Multiple export formats: HTML (dark/light), TXT and CSV
- Renders all message features including: markdown, attachments, embeds, emojis, mentions, etc

## Screenshots

![channel list](.screenshots/list.png)
![rendered output](.screenshots/output.png)