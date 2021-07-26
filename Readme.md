# DiscordChatExporter

[![Build](https://github.com/Tyrrrz/DiscordChatExporter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/DiscordChatExporter/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/DiscordChatExporter/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/DiscordChatExporter)
[![Release](https://img.shields.io/github/release/Tyrrrz/DiscordChatExporter.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Downloads](https://img.shields.io/github/downloads/Tyrrrz/DiscordChatExporter/total.svg)](https://github.com/Tyrrrz/DiscordChatExporter/releases)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

✅ **Project status: active**.

DiscordChatExporter can be used to export message history from a [Discord](https://discord.com) channel to a file.
It works with direct messages, group messages, and server channels, and supports Discord's dialect of markdown as well as all other rich media features.

❓ **If you have questions or issues, please refer to the [wiki](https://github.com/Tyrrrz/DiscordChatExporter/wiki)**.

💬 **If you want to chat, join my [Discord server](https://discord.gg/2SUWKFnHSm)**.

## Download

This application comes in two flavors: graphical user interface (**GUI**) and command line interface (**CLI**).
The following table lists all available download options:

<table>
  <thead>
    <tr>
      <th></th>
      <th>Downloads</th>
      <th>Supported OS</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><b>GUI</b></td>
      <td>
        <ul>
          <li>🟢 <b><a href="https://github.com/Tyrrrz/DiscordChatExporter/releases/latest">Stable release</a></b> (<code>DiscordChatExporter.zip</code>)</li>
          <li>🟠 <a href="https://github.com/Tyrrrz/DiscordChatExporter/actions/workflows/CI.yml">CI build</a> (<code>DiscordChatExporter.zip</code>)</li>
        </ul>
      </td>
      <td>
        <ul>
          <li>Windows <b>7</b> or higher</li>
        </ul>
      </td>
    </tr>
    <tr>
      <td><b>CLI</b></td>
      <td>
        <ul>
          <li>🟢 <b><a href="https://github.com/Tyrrrz/DiscordChatExporter/releases/latest">Stable release</a></b> (<code>DiscordChatExporter.CLI.zip</code>) ⚙️</li>
          <li>🟠 <a href="https://github.com/Tyrrrz/DiscordChatExporter/actions/workflows/CI.yml">CI build</a> (<code>DiscordChatExporter.CLI.zip</code>) ⚙️</li>
          <li>🐋 <a href="https://hub.docker.com/r/tyrrrz/discordchatexporter">Docker</a> (<code>tyrrrz/discordchatexporter</code>)</li>
          <li>📦 <a href="https://aur.archlinux.org/packages/discord-chat-exporter-cli">AUR</a> (<code>discord-chat-exporter-cli</code>) 🦄</li>
        </ul>
      </td>
      <td>
        <ul>
          <li>Windows <b>7</b> or higher</li>
          <li>macOS <b>10.13 (High Sierra)</b> or higher</li>
          <li>Linux (multiple distros)</li>
        </ul>
      </td>
    </tr>
  </tbody>
</table>

- ⚙️ - Requires .NET runtime to be installed manually:
  - [.NET Core v3.1 Runtime for **Windows x64**](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x64-installer)
  - [.NET Core v3.1 Runtime for **Windows x86**](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.0-windows-x86-installer)
  - [.NET Core v3.1 Runtime for **macOS x64**](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-3.1.0-macos-x64-installer)
  - [.NET Core v3.1 Runtime for **Linux**](https://docs.microsoft.com/en-us/dotnet/core/install/linux) (find your distro)
- 🦄 - Community-maintained resource

## Features

- Graphical user interface (Windows)
- Command line interface (Windows, Linux, macOS)
- Support for both user and bot tokens
- Support for Discord's dialect of markdown
- Support for Discord's message filter syntax
- Support for attachments, embeds, emoji, and other rich media features
- Multiple output formats: HTML (dark/light), TXT, CSV, JSON
- File partitioning, date ranges, and other export options
- Exports messages on the fly without buffering in-memory

## Screenshots

![channel list](.screenshots/list.png)
![rendered output](.screenshots/output.png)

## Building the project locally

Prerequisites:

- [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- [.NET 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/3.1) (temporarily as the app still targets .NET 3.1, but requires .NET 5.0 to build)
- _(Recommended)_ C#/.NET IDE, such as [JetBrains Rider](https://www.jetbrains.com/rider), [VS Code](https://code.visualstudio.com/docs/languages/csharp), or [Visual Studio](https://visualstudio.microsoft.com).

To build the entire solution run the following command in the root of the repository:

```sh
> dotnet build
```

This will generate runtime artifacts for each project:

```plaintext
./DiscordChatExporter.Gui/bin/[Debug|Release]/[runtime]/*
./DiscordChatExporter.Cli/bin/[Debug|Release]/[runtime]/*
```

You can also build and run a specific project directly.
To do that, navigate to its directory and use `dotnet run`:

```sh
> cd DiscordChatExporter.Gui
> dotnet run
```