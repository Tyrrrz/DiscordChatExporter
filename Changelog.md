### v2.12.1 (12-Apr-2019)

- [GUI] Fixed an issue where the app crashed when trying to export multiple channels.
- [HTML] Fixed an issue where some of the emojis were rendered via Twemoji while Discord renders them as plain text.

### v2.12 (11-Apr-2019)

- Improved markdown parsing performance which speeds up the final stage of exporting by around 4.5 times.
- Improved performance when exporting as PlainText or CSV.
- [GUI] Fixed an issue where the app would freeze up and report as "not responding" during the final stage of exporting.
- [CLI] Fixed an issue where it would say "Completed" after exporting a chat log, even if the process failed.

### v2.11 (14-Mar-2019)

- [HTML] Added syntax highlighting for multiline code blocks via Highlight.js.
- Added retry policy for all 5xx status codes to prevent random crashes.

### v2.10.2 (10-Mar-2019)

- [HTML] Updated message grouping algorithm to make it the same as in Discord. Removed "message group limit" setting and parameter.
- [HTML] Added small margin at the end of the chatlog so it doesn't look like it was truncated.
- [CLI] Fixed an issue where the app would crash if stdout was redirected. Progress will not be reported in such cases.

### v2.10.1 (06-Mar-2019)

- [HTML] Fixed an issue where multiple emojis on a single line would get rendered as one emoji.
- [HTML] Fixed an issue where an open square bracket character would incorrectly render as an emoji.
- [HTML] Added alt text to emoji images so that they will be rendered using default font when Twemoji images are not available.

### v2.10 (03-Mar-2019)

- [HTML] Reworked the entire markdown parsing engine which fixes numerous rendering inconsistencies (over 15 issues fixed).
- [HTML] Added support for animated emojis.
- [HTML] Standard emojis are now rendered using Twemoji, the same image set that Discord uses.
- [HTML] Fixed an issue where video attachments would render incorrectly.
- [HTML] Minor fixes in styling.
- [GUI] Aggregated export progress is now shown in the taskbar.
- [GUI] Checkmarks are now displayed next to channels that are selected.
- [CLI] Added progress reporting when exporting chat logs.

### v2.9.1 (10-Feb-2019)

- Fixed an issue where some of the last messages in a channel would sometimes not get exported.

### v2.9 (09-Feb-2019)

- Added categories to channel list in GUI. Channels are now also grouped by category.
- Added ability to export multiple channels at once in GUI by selecting them with shift or ctrl keys.
- Added CLI command to export all available channels in a guild (`exportguild`).
- Added CLI command to export all direct message channels (`exportdm`).
- Added retry policy for 503 status code errors to prevent random crashes.
- Changed how channels are exported in GUI. Now you need to select them first and then click a button to export. You can also double click a single channel to export it as before.
- Improved default file name generation by adding date ranges and channel ID to the file name.
- Improved naming of files exported with partitioning.
- Improved usage instructions.
- Improved GUI layout and styling.
- Fixed an issue where dialogs would sometimes appear over other windows.
- Fixed an issue where launching DiscordChatExporter from Windows Start Menu would make it crash.

### v2.8 (03-Nov-2018)

- Added support for partitioning which lets you split the output into multiple files by setting the partition message limit (`-p` parameter for CLI).
- Exported file will no longer open automatically on completion.
- Reduced amount of memory used during exporting.
- Disabled message grouping in PlainText export.
- Improved encoding of newlines in CSV export.
- Improved error messages in the GUI app during crashes.

### v2.7 (14-Sep-2018)

- Updated instructions on how to obtain the user token.
- Expanded CLI with new commands: `channels` (get a list of channels in a guild), `dm` (get a list of DM channels), `guilds` (get a list of guilds), on top of `export` (export chatlog).
- Improved help text and error messages in CLI.
- In CLI, the file name will be automatically generated if the provided output file path is a directory.

### v2.6 (25-Jul-2018)

- Added support for bot tokens as an alternative to user tokens. For GUI, use the button in the top-left to switch between user and bot token. For CLI, pass the `--bot` switch to indicate that the given token is a bot token.
- Added support for animated avatars.

### v2.5.1 (01-Jul-2018)

- Fixed a bug that would prevent timestamps from rendering properly in CSV and PlainText exports.
- Fixed incorrect layout in CSV export.
- Improved readability of PlainText export.

### v2.5 (28-Jun-2018)

- Added support for embeds and webhooks (HTML only).
- Implemented progress reporting when downloading messages.
- Added support for reactions (HTML only).
- Selected date range is now shown in the export (HTML and PlainText).
- Added placeholders for guild icons to display while the icons are loading.
- Added support for jumbo emojis (custom emojis only).
- Added basic retry policy for 429 error responses.
- Added validation for from/to dates in the UI which prevents choosing invalid date ranges.
- Added an error message when trying to export a deleted channel.

### v2.4.1 (15-Apr-2018)

- Added CSV export format.
- Channels are now ordered by name in the UI.
- Exported file is now always opened upon completion.

### v2.4 (08-Apr-2018)

- Added channel topic to output.
- Added a command line interface alternative to the current graphical user interface.
- Implemented application auto-update (can be disabled in settings).
- Improved some GUI elements and replaced some dialogs with notifications.
- Fixed a lot of issues with the markdown processor.
- Fixed DM group chats that have a custom name being shown as blank in the list.

### v2.3 (27-Oct-2017)

- Improved message date filtering, it's now marginally faster.
- Fixed underscores not recognized as italics in markdown.
- Added support for custom emojis.
- Added support for user and role mentions.
- Added support for channel mentions.
- Fixed text in pre blocks not being wrapped correctly.
- Added workaround for non-default message types.