# DiscordChatExporter

[![Build](https://github.com/Tyrrrz/DiscordChatExporter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/DiscordChatExporter/actions)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)
[![Extra Services](https://img.shields.io/badge/extra%20services-xs:code-blue.svg)](https://xscode.com/Tyrrrz/DiscordChatExporter)

✅ **Project status: active**.

DiscordChatExporter can be used to export message history from a [Discord](https://discord.com) channel to a file. It works with direct messages, group messages, and server channels, and supports Discord's dialect of markdown as well as all other rich media features.

❓ **If you have questions or issues, please check out the [wiki](https://github.com/Tyrrrz/DiscordChatExporter/wiki)**.

## Download

DiscordChatExporter comes in two flavors: graphical user interface (**GUI**) and command line interface (**CLI**).

- **Graphical user interface** (Windows-only)
  - Recommended: [Download `DiscordChatExporter.zip` from latest release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) [requires .NET runtime]
  - [Download `DiscordChatExporter.zip` from latest CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions?query=workflow%3ACI) [requires .NET runtime]
    
- **Command line interface** (Windows, Linux, and macOS)
  - Recommended: [Download `DiscordChatExporter.CLI.zip` from latest release](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) [requires .NET runtime]
  - [Download `DiscordChatExporter.CLI.zip` from latest CI build](https://github.com/Tyrrrz/DiscordChatExporter/actions?query=workflow%3ACI) [requires .NET runtime]
  - [Install from Docker Hub](https://hub.docker.com/r/tyrrrz/discordchatexporter): `docker pull tyrrrz/discordchatexporter`
  - [Install from Arch Linux User Repository](https://aur.archlinux.org/packages/discord-chat-exporter-cli): `yay -S discord-chat-exporter-cli` [community-maintained]

**Important**: This application requires **.NET Core v3.1** runtime in order to run. Some download options come with the runtime pre-packaged, but those marked with `[requires .NET runtime]` do not. To install the runtime, find a suitable download option below:

- Windows: [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x64-installer) | [x86](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x86-installer)
- macOS: [x64](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-3.1.0-macos-x64-installer)
- Linux: [find your distribution here](https://docs.microsoft.com/en-us/dotnet/core/install/linux)

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
