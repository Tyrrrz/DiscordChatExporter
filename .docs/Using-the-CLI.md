# Using the CLI

## Step 1

After extracting the `.zip` archive, open your preferred terminal.

## Step 2

Change the current directory to DCE's folder with `cd C:\path\to\DiscordChatExporter` (`cd /path/to/DiscordChatExporter` on **MacOS** and **Linux**), then press ENTER to run the command.

**Windows** users can quickly get the folder's path by clicking the address bar while inside the folder.
![Copy path from Explorer](https://i.imgur.com/XncnhC2.gif)

**macOS** users can press Command+Option+C (⌘⌥C) while inside the folder (or selecting it) to copy its path to the clipboard.

You can also drag and drop the folder on **every platform**.
![Drag and drop folder](https://i.imgur.com/sOpZQAb.gif)

## Step 3

Now we're ready to run the commands.

Type the following command in your terminal of choice, then press ENTER to run it. This will list all available subcommands and options.

```console
./dce
```

> **Note**:
> On Windows, use `dce` instead of `./dce`.

> **Docker** users, please refer to the [Docker usage instructions](Docker.md).

## CLI commands

| Command           | Description                                                          |
| ----------------- | -------------------------------------------------------------------- |
| export            | Exports one or more channels                                         |
| list channels     | Outputs the list of channels in the given server(s)                  |
| list channels dm  | Outputs the list of direct message channels                          |
| list servers      | Outputs the list of accessible servers                               |
| list unwrap       | Resolves categories in a channel list to their child channels        |
| guide             | Explains how to obtain token, server, and channel ID                 |

To use the commands, you'll need a token. For the instructions on how to get a token, please refer to [this page](Token-and-IDs.md), or run `./dce guide`.

To pass the token, use the `-t|--token` option:

```console
./dce export 53555 -t "mfa.Ifrn"
```

Alternatively, you can set the `DISCORD_TOKEN` environment variable and omit `-t`:

**Linux/macOS:**

```console
export DISCORD_TOKEN="mfa.Ifrn"
```

**Windows:**

```console
set DISCORD_TOKEN=mfa.Ifrn
```

The pipeline examples in this guide assume `DISCORD_TOKEN` is already set.

To get help with a specific command, run:

```console
./dce command --help
```

For example, to figure out how to use the `export` command, run:

```console
./dce export --help
```

## Export a specific channel

You can quickly export with DCE's default settings by providing the channel ID as a positional argument and `-t|--token`.

```console
./dce export 53555 -t "mfa.Ifrn"
```

#### Changing the format

You can change the export format to `HtmlDark`, `HtmlLight`, `PlainText` `Json` or `Csv` with `-f|--format`. The default
format is `HtmlDark`.

```console
./dce export 53555 -t "mfa.Ifrn" -f Json
```

#### Changing the output filename

You can change the filename by using `-o|--output`. e.g. for the `HTML` format:

```console
./dce export 53555 -t "mfa.Ifrn" -o myserver.html
```

#### Changing the output directory

You can change the export directory by using `-o|--output` and providing a path that ends with a slash or does not have a file
extension.
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
./dce export 53555 -t "mfa.Ifrn" -o "C:\Discord Exports"
```

#### Changing the filename and output directory

You can change both the filename and export directory by using `-o|--output`.
Note that the filename must have an extension, otherwise it will be considered a directory name.
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
./dce export 53555 -t "mfa.Ifrn" -o "C:\Discord Exports\myserver.html"
```

#### Generating the filename and output directory dynamically

You can use template tokens to generate the output file path based on the server and channel metadata.

```console
./dce export 53555 -t "mfa.Ifrn" -o "C:\Discord Exports\%G\%T\%C.html"
```

Assuming you are exporting a channel named `"my-channel"` in the `"Text channels"` category from a server
called `"My server"`, you will get the following output file
path: `C:\Discord Exports\My server\Text channels\my-channel.html`

Here is the full list of supported template tokens:

- `%g` - server ID
- `%G` - server name
- `%t` - category ID
- `%T` - category name
- `%c` - channel ID
- `%C` - channel name
- `%p` - channel position
- `%P` - category position
- `%a` - the "after" date
- `%b` - the "before" date
- `%d` - the current date
- `%%` - escapes `%`

#### Partitioning

You can use partitioning to split files after a given number of messages or file size.
For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.

```console
./dce export 53555 -t "mfa.Ifrn" -p 10
```

A 45 MB channel set to be partitioned every 20 MB will output 3 files.

```console
./dce export 53555 -t "mfa.Ifrn" -p 20mb
```

#### Downloading assets

If this option is set, the export will include additional files such as user avatars, attached files, images, etc.
Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be
downloaded when using the plain text (TXT) export format.
A folder containing the assets will be created along with the exported chat. They must be kept together.

```console
./dce export 53555 -t "mfa.Ifrn" --media
```

#### Reusing assets

Previously downloaded assets can be reused to skip redundant downloads as long as the chat is always exported to the
same folder. Using this option can speed up future exports. This option requires the `--media` option.

```console
./dce export 53555 -t "mfa.Ifrn" --media --reuse-media
```

#### Changing the media directory

By default, the media directory is created alongside the exported chat. You can change this by using `--media-dir` and
providing a path that ends with a slash. All of the exported media will be stored in this directory.

```console
./dce export 53555 -t "mfa.Ifrn" --media --media-dir "C:\Discord Media"
```

#### Changing the date format

You can customize how dates are formatted in the exported files by using `--locale` and inserting one of Discord's
locales. The default locale is `en-US`.

```console
./dce export 53555 -t "mfa.Ifrn" --locale "de-DE"
```

#### Date ranges

**Messages sent before a date**
Use `--before` to export messages sent before the provided date. E.g. messages sent before September 18th, 2019:

```console
./dce export 53555 -t "mfa.Ifrn" --before 2019-09-18
```

**Messages sent after a date**
Use `--after` to export messages sent after the provided date. E.g. messages sent after September 17th, 2019 11:34 PM:

```console
./dce export 53555 -t "mfa.Ifrn" --after "2019-09-17 23:34"
```

**Messages sent in a date range**
Use `--before` and `--after` to export messages sent during the provided date range. E.g. messages sent between
September 17th, 2019 11:34 PM and September 18th:

```console
./dce export 53555 -t "mfa.Ifrn" --after "2019-09-17 23:34" --before "2019-09-18"
```

You can try different formats like `17-SEP-2019 11:34 PM` or even refine your ranges down to
milliseconds `17-SEP-2019 23:45:30.6170`!
Don't forget to quote (") the date if it has spaces!
More info about .NET date
formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

#### Filtering messages

Use `--filter` to filter what messages are included in the export.

```console
./dce export 53555 -t "mfa.Ifrn" --filter "from:Tyrrrz has:image"
```

Documentation on message filter syntax can be found [here](https://github.com/Tyrrrz/DiscordChatExporter/blob/prime/.docs/Message-filters.md).

### Export channels from a specific server

> [!IMPORTANT]
> The following examples assume `DISCORD_TOKEN` is already set. See [CLI commands](#cli-commands) for instructions.

To export all channels in a specific server, use `list channels` to list channels and pipe the result to `export`.

**Linux/macOS:**

```console
./dce list channels 21814 | ./dce export
```

**Windows:**

```console
dce list channels 21814 | dce export
```

You can also list channels for multiple servers at once:

```console
./dce list channels 21814 35930 | ./dce export
```

#### Including threads

By default, threads are not included. You can change this behavior by passing `--include-threads` to the `list channels` command. It has possible values of `none`, `active`, or `all`, indicating which threads should be included. To include both active and archived threads, use `--include-threads all`.

```console
./dce list channels 21814 --include-threads all | ./dce export
```

#### Including voice channels

By default, voice channels are included. You can change this behavior by passing `--include-vc false` to the `list channels` command.

```console
./dce list channels 21814 --include-vc false | ./dce export
```

### Export all DMs

> [!IMPORTANT]
> The following examples assume `DISCORD_TOKEN` is already set. See [CLI commands](#cli-commands) for instructions.

To export all DMs:

**Linux/macOS:**

```console
./dce list channels dm | ./dce export
```

**Windows:**

```console
dce list channels dm | dce export
```

### List channels in a server

To list the channels available in a specific server, use the `list channels` command and provide the server ID as an argument:

```console
./dce list channels 21814 -t "mfa.Ifrn"
```

The `list channels` command outputs a JSON array of channel objects. You can pipe this directly to the `export` command:

```console
./dce list channels 21814 | ./dce export
```

### List direct message channels

To list all DM channels accessible to the current account, use the `list channels dm` command:

```console
./dce list channels dm -t "mfa.Ifrn"
```

The `list channels dm` command outputs a JSON array of channel objects. You can pipe this directly to the `export` command:

```console
./dce list channels dm | ./dce export
```

### Unwrap categories

To resolve category channels in a list to their child channels, use the `list unwrap` command:

```console
./dce list channels 21814 | ./dce list unwrap | ./dce export
```

### List servers

To list all servers accessible by the current account, use the `list servers` command:

```console
./dce list servers -t "mfa.Ifrn" > C:\path\to\output.txt
```
