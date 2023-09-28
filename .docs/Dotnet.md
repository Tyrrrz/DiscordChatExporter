# Install .NET runtime

**.NET v7.0 Runtime** is required by **DiscordChatExporter v2.37 and newer**.

If you are using GUI flavor of DiscordChatExporter, the runtime will be installed automatically (since v2.27.1).
If you are using the CLI from Docker, installing the runtime on the host machine is not needed.

- Install
  - [Windows and macOS](#windows-and-macos)
  - [Linux](#linux)
  - [Docker](#docker)
- [Verify installation](#verify-installation-optional)

## Windows and macOS

**Download directly from one of the links:**

- [.NET v7.0 Runtime for **Windows x64**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-7.0.11-windows-x64-installer)
- [.NET v7.0 Runtime for **Windows x86**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-7.0.11-windows-x86-installer)
- [.NET v7.0 Runtime for **Macs with Apple silicon**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-7.0.11-macos-arm64-installer)
- [.NET v7.0 Runtime for **Macs with an Intel processor**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-7.0.11-macos-x64-installer)

> **Note**:
> [How can I tell if my computer is running a 32-bit (x86) or a 64-bit (x64) version of Windows?](https://support.microsoft.com/help/15056/windows-32-64-bit-faq)

> **Note**:
> [Is my Mac using an Intel processor or Apple silicon?](https://support.apple.com/HT211814)

**Otherwise:**
[Check out the latest version on this page](https://dotnet.microsoft.com/download/dotnet/7.0).
Look for the download link in the '**.NET Runtime**' section, under **Installers**:

[![Screenshot from download page](https://i.imgur.com/t3PB3NC.png)](https://dotnet.microsoft.com/download/dotnet/7.0)

## Linux

Installing the .NET Runtime works differently depending on which Linux distribution you use.

Check out the instructions for your distro:

- [Ubuntu](https://docs.microsoft.com/dotnet/core/install/linux-ubuntu)
- [Debian](https://docs.microsoft.com/dotnet/core/install/linux-debian)
- [Fedora](https://docs.microsoft.com/dotnet/core/install/linux-fedora)

For other distros, please check the _'Install on Linux'_ menu on the left of [this page](https://docs.microsoft.com/dotnet/core/install/linux).

## Docker

Installing the .NET Runtime is not needed. Please refer to the [Docker usage instructions](https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs/Docker.md) page for more information.

## Verify installation (optional)

You can check which version of **.NET Runtime** is installed by following these instructions:

### Step 1

**Windows:** Open Command Prompt (aka `cmd`)

**macOS and Linux:** Open `Terminal`

### Step 2

Run `dotnet --info`

If the **.NET Runtime** is correctly installed, the command will output the following:

```console
.NET runtimes installed:
  Microsoft.NETCore.App 7.0.x [C:\path\to\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.WindowsDesktop.App 7.0.x [C:\path\to\dotnet\shared\Microsoft.WindowsDesktop.App]
```
