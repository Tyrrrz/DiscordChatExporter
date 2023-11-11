# Install .NET runtime

**.NET v7.0 Runtime** is required by **DiscordChatExporter v2.37 and newer**.

> **Note**:
> Installing the **.NET Runtime** is not required if you running **DiscordChatExporter** using [Docker](Docker.md).

## Windows

Both the GUI and the CLI flavors of **DiscordChatExporter** for Windows come with a bootstrapper that should automatically install all the required prerequisites. Simply run the application and follow the presented instructions.

If, for some reason, you need to install the runtime manually, use one of the download links below:

- [.NET v7.0 Runtime for **Windows x64**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-7.0.13-windows-x64-installer)
- [.NET v7.0 Runtime for **Windows x86**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-7.0.13-windows-x86-installer)
- [.NET v7.0 Runtime for **Windows arm64**](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.13-windows-arm64-installer)

> **Note**:
> [How can I tell if my computer is running a 32-bit (x86) or a 64-bit (x64) version of Windows?](https://support.microsoft.com/help/15056/windows-32-64-bit-faq)

## macOS

Use one of the download links below:

- [.NET v7.0 Runtime for **Macs with Apple silicon**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-7.0.13-macos-arm64-installer)
- [.NET v7.0 Runtime for **Macs with an Intel processor**](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-7.0.13-macos-x64-installer)

> **Note**:
> [Is my Mac using an Intel processor or Apple silicon?](https://support.apple.com/HT211814)

## Linux

Installing the .NET Runtime works differently depending on which Linux distribution you use. Check out the instructions for your distro:

- [Ubuntu](https://docs.microsoft.com/dotnet/core/install/linux-ubuntu)
- [Debian](https://docs.microsoft.com/dotnet/core/install/linux-debian)
- [Fedora](https://docs.microsoft.com/dotnet/core/install/linux-fedora)

For other distros, please check the _'Install on Linux'_ menu on the left of [this page](https://docs.microsoft.com/dotnet/core/install/linux).

## Verify installation (optional)

You can check which version of **.NET Runtime** is installed by running the following command in a terminal:

```console
dotnet --info
```

If the **.NET Runtime** is correctly installed, the command will output something similar to the following:

```console
.NET runtimes installed:
  Microsoft.NETCore.App 7.0.x [C:\path\to\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.WindowsDesktop.App 7.0.x [C:\path\to\dotnet\shared\Microsoft.WindowsDesktop.App]
```
