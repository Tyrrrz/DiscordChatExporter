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