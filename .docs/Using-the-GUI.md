# Using the GUI

## Video tutorial

[![Video tutorial](https://i.ytimg.com/vi/jjtu0VQXV7I/hqdefault.jpg)](https://youtube.com/watch?v=jjtu0VQXV7I)

> Video by [NoIntro Tutorials](https://youtube.com/channel/UCFezKSxdNKJe77-hYiuXu3Q).

## Guide

### Step 1

After extracting the `.zip`, open `DiscordChatExporter.exe`

### Step 2

Please refer to the on-screen instructions to get your token, then paste your token in the upper text box and hit ENTER or click the arrow (→).

> **Warning**:
> **Never share your token!**
> A token gives full access to an account, treat it like a password.

<img src="https://i.imgur.com/SuLQ5tZ.png" height="400"/>

### Step 3

DCE will display your Direct Messages and a sidebar with your server list. Select the channel you would like to export, then click the ![Screenshot](https://i.imgur.com/dnTOlDa.png) button to continue.

> **Note**:
> You can export multiple channels at once by holding `CTRL` or `SHIFT` while selecting.
> You can also double-click a channel to export it without clicking the ![Screenshot](https://i.imgur.com/dnTOlDa.png) button.

<img src="https://i.imgur.com/JHMFRh2.png" height="400"/>

### Step 4

In this screen you can customize the following:

- **Output path** - The folder where the exported chat(s) will be saved.

- **Export format** - HTML (Dark), HTML (Light), TXT, CSV and JSON

- **Date range (after/before)** (Optional) - If set, only messages sent in the provided date range will be exported. Only one value (either after or before) is required if you want to use this option.
  > **Note**:
  > Please note that the time defaults to **12:00 AM** (midnight/00:00). This means that if you choose to export between Sep 17th and Sep 18th, messages from Sep 18th won't be exported.

- **Partition limit** (Optional) - Split output into partitions, each limited to this number of messages (e.g. 100) or file size (e.g. 10mb). For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.

- **Message Filter** (Optional) - Special notation for filtering the messages that get included in the export. See [Message filters](Message-filters.md) for more info.

- **Format markdown** (Optional) - Disable markdown processing when exporting. You can use this to produce JSON or plain text exports without unwrapping mentions, custom emoji, and certain other special tokens.

- **Download assets** (Optional) - If this option is set, the export will include additional files such as user avatars, attached files, images, etc. Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using the plain text (TXT) export format. A folder containing the assets will be created along with the exported chat. They must be kept together.

- **Reuse assets** (Optional) - If this option is set, the export will reuse already downloaded assets to skip redundant requests. This option is only available when **Download assets** is enabled.

- **Assets directory path** (Optional) - If this option is set, the export will use the specified directory to store assets from all exported channels in the same place.

> **Note**:
> You need to scroll down to see all available options.

## Settings

- **Auto-update**
Perform automatic updates on every launch.
Default: Enabled

  > **Note**:
  > Keep this option enabled to receive the latest features and bug fixes!

- **Dark mode**
Use darker colors in the UI (User Interface).
Default: Disabled

- **Persist token**
Persist last used token between sessions.
Default: Enabled

- **Show threads**
Controls whether threads are shown in the channel list.
Default: none

- **Locale**
Customize how dates are formatted in the exported files.

- **Date format**
Customize how dates are formatted in the exported files in the settings menu ().

- **Parallel limit**
The number of channels that will be exported at the same time.
Default: 1

- **Normalize to UTC**
Convert all dates to UTC before exporting.

  > **Note**:
  > Try to keep this number low so that your account doesn't get flagged.
