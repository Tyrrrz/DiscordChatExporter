# DiscordChatExporter

[![Build](https://github.com/Tyrrrz/DiscordChatExporter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/DiscordChatExporter/actions)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)
[![Extra Services](https://img.shields.io/badge/extra%20services-xs:code-blue.svg)](https://xscode.com/Tyrrrz/DiscordChatExporter)

**Project status: active**.

DiscordChatExporter can be used to export message history from a [Discord](https://discord.com) channel to a file. It works with direct messages, group messages, server channels, supports Discord's dialect of markdown and all other rich media features.

Have questions or need help? Check out the [wiki](https://github.com/Tyrrrz/DiscordChatExporter/wiki).

## Download

- **[Latest release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest)**
- [Docker image](https://hub.docker.com/r/tyrrrz/discordchatexporter): `docker pull tyrrrz/discordchatexporter`
- [AUR](https://aur.archlinux.org/packages/discord-chat-exporter-cli/): `yay -S discord-chat-exporter-cli`
- [CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions)

Note: This application requires .NET Core runtime in order to run:

- .NET Core runtime for Windows: [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x64-installer) | [x86](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x86-installer)
- .NET Core runtime for macOS (for CLI version only): [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-3.1.0-macos-x64-installer)
- .NET Core runtime for Linux (for CLI version only): [Ubuntu/Debian](https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1904#install-the-net-core-runtime)

## Features

- Graphical user interface (Windows)
- Command line interface (Windows, Linux, macOS)
- Works with both user and bot tokens
- Exports on the fly without buffering messages in memory
- Allows exporting messages in a specified range
- Supports file partitioning based on message count
- Uses custom markdown parser compatible with Discord syntax
- Handles all rich media features, including attachments, embeds, emojis, etc
- Renders to HTML (dark & light), TXT, CSV, JSON

## Screenshots

![channel list](.screenshots/list.png)
![rendered output](.screenshots/output.png)
