
# Getting started

Welcome to the getting started page!  
Here you'll learn how to use every **DiscordChatExporter** (DCE for short) feature.  
For other things you can do with DCE, check out the [Guides](https://github.com/Tyrrrz/DiscordChatExporter/wiki#guides) section.

If you still have unanswered questions after reading this page or if you have encountered a problem, please visit our [FAQ & Troubleshooting](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Troubleshooting) section.  

The information presented on this page is valid for **all** platforms.

**Index**
* [GUI or CLI?](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#gui-or-cli)
* [Using the GUI](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#using-the-gui)
* [Using the CLI](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#using-the-cli)
  * [Command list](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#dcecli-commands)
* [File formats](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats)
  * [HTML](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#html)
  * [Plain Text](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#plain-text)
  * [JSON](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#json)
  * [CSV](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#csv)


# GUI or CLI?
![](https://i.imgur.com/j9OTxRB.png)
**DCE** has two different versions: 
* **Graphical User Interface** (**GUI**) - it's the preferred version for newcomers as it is easy to use.  
You can get it by [downloading](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) the `DiscordChatExporter.zip` file.    
* **Command Line Interface** (**CLI**) - offers greater flexibility and more features for advanced users, such as export scheduling, ID lists, and more specific date ranges.  
You can get it by [downloading](https://github.com/Tyrrrz/DiscordChatExporter/releases/latest) the `DiscordChatExporter.CLI.zip` file.

If you're not comfortable with **Windows'** Command Line (cmd), please choose the GUI.  
[**macOS**](https://github.com/Tyrrrz/DiscordChatExporter/wiki/macOS-usage-instructions), [**Linux**](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Linux-usage-instructions) and [**Docker**](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Docker-usage-instructions) users can only use the CLI version.  

**(Skip to [Using the CLI](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#using-the-cli) or [File Formats](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats))**
# Using the GUI
### Video tutorial

[![Video tutorial](https://i.ytimg.com/vi/jjtu0VQXV7I/hqdefault.jpg)](https://youtube.com/watch?v=jjtu0VQXV7I)

> Video by [NoIntro Tutorials](https://youtube.com/channel/UCFezKSxdNKJe77-hYiuXu3Q).

### Guide
**1.** After extracting the `.zip`, open `DiscordChatExporter.exe`

**2.** Please refer to the on-screen instructions to get your token, then paste your token in the upper text box and hit ENTER or click the arrow (→).   
**NEVER SHARE YOUR TOKEN**. A token gives <u>full access</u> to an account, treat it like a password.

<img src="https://i.imgur.com/SuLQ5tZ.png" height="400"/>

**3.** DCE will display your Direct Messages and a sidebar with your server list. Select the channel you would like to export, then click the ![](https://i.imgur.com/dnTOlDa.png) button to continue.  
> 💡 Tip: You can export multiple channels at once by holding `CTRL` or `SHIFT` while selecting.  
> You can also double-click a channel to export it without clicking the ![](https://i.imgur.com/dnTOlDa.png) button.


<img src="https://i.imgur.com/JHMFRh2.png" height="400"/>

**4.** In this screen you can customize the following:
* **Export format** - HTML (Dark), HTML (Light), TXT, CSV and JSON

Click the menu button (☰) to see more options:

* **Date range (after/before)** (Optional) - If set, only messages sent in the provided date range will be exported. Only one value (either after or before) is required if you want to use this option.
  > Please note that the time defaults to **12:00 AM** (midnight/00:00). This means that if you choose to export between Sep 17th and Sep 18th, messages from Sep 18th won't be exported.
* **Filter** (Optional) - Special notation for filtering the messages that get included in the export. See [Message filters](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Message-filters) for more info.
* **Messages per partition** (Optional) - Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb). For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.
* **Download assets** (Optional) - If this option is set, the export will include additional files such as user avatars, attached files, images, etc.  
Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using the plain text (TXT) export format.  
A folder containing the assets will be created along with the exported chat. They must be kept together.

**5.** Click `EXPORT` to export.

<img src="https://i.imgur.com/KPgMH2D.png" height="400"/><img src="https://i.imgur.com/mtUFojS.png" height="400"/>

### Settings

**Auto-updates**  
Perform automatic updates on every launch.  
Default: Enabled  
> 💡 Tip: Keep this option enabled to receive the latest features and bug fixes!

**Dark mode**  
Use darker colors in the UI (User Interface).  
Default: Disabled

**Save token**  
Persist last used token between sessions.  
Default: Enabled

**Reuse downloaded assets**  
Reuse already downloaded assets to skip redundant requests.  
Default: Disabled

**Date format**  
You can customize how dates are formatted in the exported files in the settings menu ().  
* The default one is `17-Sep-19 11:34 PM` - `dd-MMM-yy hh:mm tt`
* To change it to `09-17-19 11:34 PM` use `MM-dd-yy hh:mm tt`
* To change it to `17/09/2019 23:34` use `dd/MM/yyyy HH:mm`
* To change it to `2019-09-17 23:34:05.6534` use `yyyy-MM-dd HH:mm:ss.ffff`
* To change it to UTC `2019-09-18 04:34:05Z` use `u`
* To display the time zone, add `K`. E.g. to change it to `17-Sep-19 11:34 PM (UTC-05:00)` use `dd-MMM-yy hh:mm tt (UTCK)`
* To change it to Unix time `1568694845` use `unix`
* To change it to Unix time in milliseconds `1568694845653` use `unixms`

More info about .NET date formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).


**Parallel limit**  
The number of channels can be exported at the same time.  
Default: 1

> 💡 Tip: Try to keep this number low so that your account doesn't get flagged.

**(Skip to [File Formats](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats))**

# Using the CLI
> Make sure you have [.NET Core installed](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Install-.NET-Core-runtime) before attempting to run the commands below.  
> **Docker** users, please refer to the [Docker usage instructions](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Docker-usage-instructions).

**1.** After extracting the `.zip`, open Command Prompt, aka `cmd` (`Terminal` on **macOS** and **Linux**).

**2.** Change the current directory to DCE's folder with `cd C:\path\to\directory`, then press ENTER to run the command.
    
**Windows** users can quickly get the directory's path by clicking the address bar while inside the folder.  
![](https://i.imgur.com/XncnhC2.gif)    
**macOS** users can select the `.exe`, hit Command+I (⌘I), and copy what's after `Where:` to get the directory  
![](https://camo.githubusercontent.com/3c51a904b0099c9f68a4797461d4a7914902fc04/68747470733a2f2f692e696d6775722e636f6d2f323975364e79782e706e67)    
You can also drag and drop the folder on **every platform**.  
![](https://i.imgur.com/sOpZQAb.gif)

**3.** Now we're ready to run the commands. The examples on this page follow the Windows file path format, change the file paths according to your system.  

Let's do the following to list DCE's options:

Type the following in Command Prompt (Terminal), then press ENTER to run it:
```powershell
dotnet DiscordChatExporter.Cli.dll
```

> **Docker** users, please refer to the [Docker usage instructions](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Docker-usage-instructions).

## DCE.CLI commands

| Command | Description |
| ------- | ----------- |
[export](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#export) | Exports a channel
[exportdm](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#exportdm) | Exports all direct message channels
[exportguild](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#exportguild) | Exports all channels within the specified server
[exportall](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#exportall) | Exports all accessible channels
[channels](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#channels) | Outputs the list of channels in the given server
[dm](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#dm) | Outputs the list of direct message channels
[guilds](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#guilds) | Outputs the list of accessible servers
[guide](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#guide) | Explains how to obtain token, guild, and channel ID

To use the commands, you'll need a token. For the instructions on how to get a token, please refer to [this page](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs), or run `dotnet DiscordChatExporter.Cli.dll guide`.  
To get help with a specific command, please run:
```powershell
dotnet DiscordChatExporter.Cli.dll command -h
```

## export

||Option | Description
|--|------- | -------------
|**\***|[-c](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Channel ID(s)](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-guild-id-or-guild-channel-id)
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||[-o](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-output-filename) | Output file or directory path
||[-f](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-format) | [Output file format](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats). Default: HtmlDark
||[--after](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges)| Only include messages sent after this date
||[--before](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges) | Only include messages sent before this date
||[-p](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#partitioning) | Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb)
||[--filter](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Message-filters) | Special notation for filtering the messages that get included in the export
||[--media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Downloading-assets) | Download assets referenced by the export (user avatars, attached files, embedded images, etc.). Default: false
||[--reuse-media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Reuse-assets) | Reuse previously downloaded assets to avoid redundant requests. Default: false.
||[--dateformat](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-formats) | Format used when writing dates.
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.

With this command you can export Server Channels and Direct Messages.

### Basic usage
You can quickly export with DCE's default settings by using just `-t token` and `-c channelid`.
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555
```

### Changing the format
You can change the export format to `HtmlDark`, `HtmlLight`, `PlainText` `Json` or `Csv` with `-f format`. The default format is `HtmlDark`.
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -f Json
```

### Changing the output filename
You can change the filename by using `-o name.ext`. e.g. for the `HTML` format:
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o myserver.html
```

### Changing the output directory
You can change the export directory by using `-o` and providing a path that ends with a slash or does not have a file extension.
If any of the folders in the path have a space in its name, escape them with quotes (").
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports"
```

### Changing the filename and output directory
You can change both the filename and export directory by using `-o directory\name.ext`.
Note that the filename must have an extension, otherwise it will be considered a directory name.
If any of the folders in the path have a space in its name, escape them with quotes (").
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\myserver.html"
```

### Generating the filename and output directory dynamically
You can use template tokens to generate the output file path based on the guild and channel metadata.

```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%G\%T\%C.html"
```

Assuming you are exporting a channel named `"my-channel"` in the `"Text channels"` category from a server called `"My server"`, you will get the following output file path: `C:\Discord Exports\My server\Text channels\my-channel.html`

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

### Partitioning
You can use partitioning to split files after a given number of messages or file size.  
For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -p 10
```

A 45mb channel set to be partitioned every 20mb will output 3 files.

```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 -p 20mb
```

### Downloading assets
If this option is set, the export will include additional files such as user avatars, attached files, images, etc.  
Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using the plain text (TXT) export format.  
A folder containing the assets will be created along with the exported chat. They must be kept together.

```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --media
```

### Reuse assets
Previously downloaded assets can be reused to skip redundant downloads as long as the chat is always exported to the same folder. Using this option can speed up future exports. This option requires the `--media` option.

```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --media --reuse-media
```


### Date formats
This setting changes how dates are formatted in the exported files.
* The default one is `17-Sep-19 11:34 PM` - `dd-MMM-yy hh:mm tt`
* To change it to `09-17-19 11:34 PM` use `MM-dd-yy hh:mm tt`
* To change it to `17/09/2019 23:34` use `dd/MM/yyyy HH:mm`
* To change it to `2019-09-17 23:34:05.6534` use `yyyy-MM-dd HH:mm:ss.ffff`
* To change it to UTC `2019-09-18 04:34:05Z` use `u`
* To display the time zone, add `K`. E.g. to change it to `17-Sep-19 11:34 PM (UTC-05:00)` use `dd-MMM-yy hh:mm tt (UTCK)`
* To change it to Unix time `1568694845` use `unix`
* To change it to Unix time in milliseconds `1568694845653` use `unixms`

More info about .NET date formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --dateformat "yyyy-MM-dd HH:mm"
```

> Special thanks to [@andrewkolos](https://github.com/andrewkolos) for adding the Unix options

### Date ranges
**Messages sent before a date**  
Use `--before` to export messages sent before the provided date. E.g. messages sent before September 18th, 2019:
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --before 2019-09-18
```
**Messages sent after a date**  
Use `--after` to export messages sent after the provided date. E.g. messages sent after September 17th, 2019 11:34 PM:
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34"
```
**Messages sent in a date range**  
Use `--before` and `--after` to export messages sent during the provided date range. E.g. messages sent between September 17th, 2019 11:34 PM and September 18th:
```powershell
dotnet DiscordChatExporter.Cli.dll export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34" --before "2019-09-18" 
```

You can try different formats like `17-SEP-2019 11:34 PM` or even refine your ranges down to milliseconds `17-SEP-2019 23:45:30.6170`!  
Don't forget to quote (") the date if it has spaces!  
More info about .NET date formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).


## exportdm
||Option | Description
|-|------- | -------------
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||--parallel| Limits how many channels can be exported in parallel. Try to keep this number low so that your account doesn't get flagged. Default: 1
||[-o](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-output-filename) | Output file or directory path
||[-f](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-format) | [Output file format](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats). Default: HtmlDark
||[--after](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges)| Only include messages sent after this date
||[--before](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges) | Only include messages sent before this date
||[-p](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#partitioning) | Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb)
||[--filter](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Message-filters) | Special notation for filtering the messages that get included in the export
||[--media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Downloading-assets) | Download assets referenced by the export (user avatars, attached files, embedded images, etc.). Default: false
||[--reuse-media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Reuse-assets) | Reuse previously downloaded assets to avoid redundant requests. Default: false.
||[--dateformat](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-formats) | Date format used in output
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command exports all your Direct Messages.


## exportguild
||Option | Description
|-|------- | -------------
|**\***|-g | [Server ID](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-guild-id-or-guild-channel-id)
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||--parallel| Limits how many channels can be exported in parallel. Try to keep this number low so that your account doesn't get flagged. Default: 1
||[-o](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-output-filename) | Output file or directory path
||[-f](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-format) | [Output file format](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats). Default: HtmlDark
||[--after](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges)| Only include messages sent after this date
||[--before](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges) | Only include messages sent before this date
||[-p](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#partitioning) | Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb)
||[--filter](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Message-filters) | Special notation for filtering the messages that get included in the export
||[--media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Downloading-assets) | Download assets referenced by the export (user avatars, attached files, embedded images, etc.). Default: false
||[--reuse-media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Reuse-assets) | Reuse previously downloaded assets to avoid redundant requests. Default: false.
||[--dateformat](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-formats) | Date format used in output
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command exports all channels of a Server.

## exportall

||Option | Description
|-|------- | -------------
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||--include-dm | Include direct message channels. Default: true
||--parallel| Limits how many channels can be exported in parallel. Try to keep this number low so that your account doesn't get flagged. Default: 1
||[-o](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-output-filename) | Output file or directory path
||[-f](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#changing-the-format) | [Output file format](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#file-formats). Default: HtmlDark
||[--after](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges)| Only include messages sent after this date
||[--before](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-ranges) | Only include messages sent before this date
||[-p](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#partitioning) | Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb)
||[--filter](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Message-filters) | Special notation for filtering the messages that get included in the export
||[--media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Downloading-assets) | Download assets referenced by the export (user avatars, attached files, embedded images, etc.). Default: false
||[--reuse-media](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#Reuse-assets) | Reuse previously downloaded assets to avoid redundant requests. Default: false.
||[--dateformat](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#date-formats) | Date format used in output
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command exports all accessible channels, including server channels and DMs.

To exclude DMs, add the `--include-dm false` option:

```powershell
dotnet DiscordChatExporter.Cli.dll exportall -t "mfa.Ifrn" --include-dm false
```

## channels
||Option | Description
|-|------- | -------------
|**\***|-g| [Server ID](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-guild-id-or-guild-channel-id)
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command outputs the server channels in the following format:  
`Channel ID | Channel Name`    
To save the output to a file, run the command below. If the file already exists, it will be overwritten.
```powershell
dotnet DiscordChatExporter.Cli.dll channels -t "mfa.Ifrn" -g 21814 > C:\path\to\output.txt
```

## dm
||Option | Description
|-|------- | -------------
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command outputs the list of Direct Messages.  
To save the output to a file, run the command with `> C:\path\…` like the example below. If the file already exists, it will be overwritten.
```powershell
dotnet DiscordChatExporter.Cli.dll dm -t "mfa.Ifrn" > C:\path\to\output.txt
```


## guilds
||Option | Description
|-|------- | -------------
|**\***|[-t](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#basic-usage) | [Authorization token](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs#how-to-get-user-token). Environment variable: `DISCORD_TOKEN`
||-h | Shows help text

> Options with an asterisk (**\***) are required. The order of the options doesn't matter.  

This command outputs a list of your Server List.  
To save the output to a file, run the command with `> C:\path\…` like the example below. If the file already exists, it will be overwritten.
```powershell
dotnet DiscordChatExporter.Cli.dll guilds -t "mfa.Ifrn" > C:\path\to\output.txt
```

## guide
This command explains [how to get your token, guild, and channel IDs](https://github.com/Tyrrrz/DiscordChatExporter/wiki/Obtaining-Token-and-Channel-IDs).

# File formats

## HTML

![](https://i.imgur.com/S7lBTkV.png)
The HTML format replicates Discord's interface, making it the most user-friendly option.  
It's the best format for attachment preview and sharing.  
You can open `.html` files with a web browser, such as Google Chrome.  
> Please note that if a picture is deleted, or if a user changes its avatar, the respective images will no longer be displayed.  
Export using the "Download referenced assets" (`--media`) option to avoid this.

## Plain Text

<img src="https://i.imgur.com/PbUyRXD.png" height="400"/>

The Plain Text format is the best option for archiving due to its small size.  
You can open `.txt` files with a text editor, such as Notepad.

## JSON

<img src="https://i.imgur.com/FAeSA4O.png" height="400"/>

The JSON format contains more technical information and is easily parsable.  
You can open `.json` files with a text editor, such as Notepad.

## CSV

![](https://i.imgur.com/VEVUsKs.png)
![](https://i.imgur.com/1vPmQqQ.png)

The CSV format allows for easy parsing of the chat log. Depending on your needs, the JSON format might be better.  
You can open `.csv` files with a text editor, such as Notepad, or a spreadsheet app, like Microsoft Excel and Google Sheets.

**([Back to top](https://github.com/Tyrrrz/DiscordChatExporter/wiki/GUI%2C-CLI-and-Formats-explained#getting-started))**

#
Further reading: [.NET date formats](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)  
Special thanks to [@Yudi](https://github.com/Yudi)