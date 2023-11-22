# Using the CLI

## Guide

> **Note**:
> Make sure you have [.NET Core installed](Dotnet.md) before attempting to run the commands below.
> **Docker** users, please refer to the [Docker usage instructions](Docker.md).

## Step 1

After extracting the `.zip`, open Command Prompt, aka `cmd` (`Terminal` on **macOS** and **Linux**).

## Step 2

Change the current directory to DCE's folder with `cd C:\path\to\directory`, then press ENTER to run the command.

**Windows** users can quickly get the directory's path by clicking the address bar while inside the folder.
![Copy path from Explorer](https://i.imgur.com/XncnhC2.gif)

**macOS** users can select the `.exe`, hit Command+I (⌘I), and copy what's after `Where:` to get the directory
![Mac info panel](https://camo.githubusercontent.com/3c51a904b0099c9f68a4797461d4a7914902fc04/68747470733a2f2f692e696d6775722e636f6d2f323975364e79782e706e67)

You can also drag and drop the folder on **every platform**.
![Drag and drop folder](https://i.imgur.com/sOpZQAb.gif)

## Step 3

Now we're ready to run the commands. The examples on this page follow the Windows file path format, change the file
paths according to your system.

Type the following in Command Prompt (Terminal), then press ENTER to run it. This will list DCE's options.

```console
dotnet DiscordChatExporter.Cli.dll
```

> **Docker** users, please refer to the [Docker usage instructions](Docker.md).

## CLI commands

| Command                 | Description                                         |
|-------------------------|-----------------------------------------------------|
| export                  | Exports a channel                                   |
| exportdm                | Exports all direct message channels                 |
| exportguild             | Exports all channels within the specified server    |
| exportall               | Exports all accessible channels                     |
| channels                | Outputs the list of channels in the given server    |
| dm                      | Outputs the list of direct message channels         |
| guilds                  | Outputs the list of accessible servers              |
| guide                   | Explains how to obtain token, guild, and channel ID |

To use the commands, you'll need a token. For the instructions on how to get a token, please refer to [this page](Token-and-IDs.md), or run `dotnet DiscordChatExporter.Cli.dll guide`.

To get help with a specific command, run:

```console
dotnet DiscordChatExporter.Cli.dll command --help
```

For example, to figure out how to use the `export` command, run:

```console
dotnet DiscordChatExporter.Cli.dll export --help
```

## Export a specific channel

You can quickly export with DCE's default settings by using just `-t token` and `-c channelid`.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555
```

#### Changing the format

You can change the export format to `HtmlDark`, `HtmlLight`, `PlainText` `Json` or `Csv` with `-f format`. The default
format is `HtmlDark`.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -f Json
```

#### Changing the output filename

You can change the filename by using `-o name.ext`. e.g. for the `HTML` format:

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o myserver.html
```

#### Changing the output directory

You can change the export directory by using `-o` and providing a path that ends with a slash or does not have a file
extension.
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports"
```

#### Changing the filename and output directory

You can change both the filename and export directory by using `-o directory\name.ext`.
Note that the filename must have an extension, otherwise it will be considered a directory name.
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\myserver.html"
```

#### Generating the filename and output directory dynamically

You can use template tokens to generate the output file path based on the guild and channel metadata.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%G\%T\%C.html"
```

Assuming you are exporting a channel named `"my-channel"` in the `"Text channels"` category from a server
called `"My server"`, you will get the following output file
path: `C:\Discord Exports\My server\Text channels\my-channel.html`

Here is the full list of supported template tokens:

- `%g` - guild ID
- `%G` - guild name
- `%t` - category ID
- `%T` - category name
- `%c` - channel ID
- `%C` - channel name
- `%p` - channel position
- `%P` - category position
- `%a` - the "after" date
- `%b` - the "before" date
- `%%` - escapes `%`

#### Partitioning

You can use partitioning to split files after a given number of messages or file size.
For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -p 10
```

A 45 MB channel set to be partitioned every 20 MB will output 3 files.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -p 20mb
```

#### Downloading assets

If this option is set, the export will include additional files such as user avatars, attached files, images, etc.
Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be
downloaded when using the plain text (TXT) export format.
A folder containing the assets will be created along with the exported chat. They must be kept together.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --media
```

#### Reusing assets

Previously downloaded assets can be reused to skip redundant downloads as long as the chat is always exported to the
same folder. Using this option can speed up future exports. This option requires the `--media` option.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --media --reuse-media
```

#### Changing the media directory

By default, the media directory is created alongside the exported chat. You can change this by using `--media-dir` and
providing a path that ends with a slash. All of the exported media will be stored in this directory.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --media --media-dir "C:\Discord Media"
```

#### Changing the date format

You can customize how dates are formatted in the exported files by using `--locale` and inserting one of Discord's
locales. The default locale is `en-US`.

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --locale "de-DE"
```

#### Date ranges

**Messages sent before a date**
Use `--before` to export messages sent before the provided date. E.g. messages sent before September 18th, 2019:

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --before 2019-09-18
```

**Messages sent after a date**
Use `--after` to export messages sent after the provided date. E.g. messages sent after September 17th, 2019 11:34 PM:

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34"
```

**Messages sent in a date range**
Use `--before` and `--after` to export messages sent during the provided date range. E.g. messages sent between
September 17th, 2019 11:34 PM and September 18th:

```console
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34" --before "2019-09-18"
```

You can try different formats like `17-SEP-2019 11:34 PM` or even refine your ranges down to
milliseconds `17-SEP-2019 23:45:30.6170`!
Don't forget to quote (") the date if it has spaces!
More info about .NET date
formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

### Export channels from a specific guild (server)

To export all channels in a specific guild, use the `exportguild` command and provide the guild ID through the `-g|--guild` option:

```console
dotnet DiscordChatExporter.Cli.dll exportguild -t "mfa.Ifrn" -g 21814
```

#### Including threads

By default, threads are not included in the export. You can change this behavior by using `--include-threads` and
specifying which threads should be included. It has possible values of `none`, `active`, or `all`, indicating which
threads should be included. To include both active and archived threads, use `--include-threads all`.

```console
dotnet DiscordChatExporter.Cli.dll exportguild -t "mfa.Ifrn" -g 21814 --include-threads all
```

#### Including voice channels

By default, voice channels are included in the export. You can change this behavior by using `--include-vc` and
specifying whether to include voice channels in the export. It has possible values of `true` or `false`, to exclude
voice channels, use `--include-vc false`.

```console
dotnet DiscordChatExporter.Cli.dll exportguild -t "mfa.Ifrn" -g 21814 --include-vc false
```

### Export all channels

To export all accessible channels, use the `exportall` command:

```console
dotnet DiscordChatExporter.Cli.dll exportall -t "mfa.Ifrn"
```

#### Excluding DMs

To exclude DMs, add the `--include-dm false` option.

```console
dotnet DiscordChatExporter.Cli.dll exportall -t "mfa.Ifrn" --include-dm false
```

### List channels in a guild (server)

To list the channels available in a specific guild, use the `channels` command and provide the guild ID through the `-g|--guild` option:

```console
dotnet DiscordChatExporter.Cli.dll channels -t "mfa.Ifrn" -g 21814
```

### List direct message channels

To list all DM channels accessible to the current account, use the `dm` command:

```console
dotnet DiscordChatExporter.Cli.dll dm -t "mfa.Ifrn"
```

### List guilds (servers)

To list all guilds accessible by the current account, use the `guilds` command:

```console
dotnet DiscordChatExporter.Cli.dll guilds -t "mfa.Ifrn" > C:\path\to\output.txt
```