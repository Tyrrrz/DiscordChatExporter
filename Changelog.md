### v2.24 (24-Oct-2020)

- [CLI] Updated to [CliFx v1.5](https://github.com/Tyrrrz/CliFx), which brings some UX improvements.
- [GUI] Added dark mode, which can be enabled in settings. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- [GUI] Improved the UX in a few minor places.
- [JSON] Added `embed.color` property to the output. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Added additional date formats: `unix` to write dates as a unix timestamp and `unixms` for the milliseconds version. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Added retry policy for downloading media content. This is mostly a workaround for transient failures that sometimes may occur when downloading assets referenced from embeds. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Added an option to reuse already downloaded media to avoid redundant network requests. It can be enabled in settings in GUI, or by passing `--reuse-media` in CLI. To facilitate this feature, the file name strategy has been changed as well -- now instead of relying on the original file name and adding `(n)` to the end in case of duplicates, a URL hash is injected in every file name. This also means that when the "reuse media" option is disabled, existing files inside the media directory may be overwritten. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))

### v2.23 (14-Sep-2020)

- [CLI] Added a command to export all channels across all servers. Use `exportall` to do it. (Thanks [@wyattscarpenter](https://github.com/wyattscarpenter))
- [HTML] Fixed an issue where Whitney fonts were not being loaded properly, causing the browser to fall back to Helvetica.
- Fixed an issue where self-contained export crashed occasionally. This usually happened when the server hosting the file did not serve the stream properly. Such files are now ignored after the first failed attempt.

### v2.22 (12-Aug-2020)

- [GUI] Improved the channel list by adding collapsible category groups. (Thanks [@CarJem Generations](https://github.com/CarJem))
- [GUI] Improved exporting options by adding a set of controls that can be used to limit the date range of the export down to minutes. Previously it was only possible to configure the date range without the time component. (Thanks [@CarJem Generations](https://github.com/CarJem))
- [HTML] Fixed an issue where the export didn't reflect changes in nicknames between messages sent by bots. This affected chat logs that contained interactions with the Tupperbox bot. (Thanks [@CarJem Generations](https://github.com/CarJem))
- [CLI] Fixed an issue where the application crashed if there were two environment variables defined that had the same name but in different case.

### v2.21.2 (30-Jul-2020)

- [GUI] When copy-pasting token, any surrounding spaces are now discarded, in addition to double quotes.
- [HTML] Changed underlying templating engine to provide higher performance and better error messages. New templating engine adds a small cold start, but any export after the first should be faster.
- Changed naming schema for downloaded media so that it follows `<original filename> (nn)` format rather than `<original filename>-salt`.
- [HTML] Fixed an issue where downloaded media was sometimes inaccessible due to reserved characters appearing in the URL.
- Fixed an issue where the application crashed when trying to download media with a file name that exceeds system's maximum allowed length.
- Fixed an issue where the application crashed when trying to download media with illegal characters in the file name.

### v2.21.1 (19-Jul-2020)

- Fixed an issue where the export crashed if an embedded image has been deleted. Such media files are now ignored.
- Changed the naming convention for downloaded media files so that the original file names are used when possible.

### v2.21 (18-Jul-2020)

- Added a new option that enables self-contained exports for all output formats. You can turn it on in the export setup dialog in GUI or using the `--media` option in CLI. When using this, the application will additionally download any media content directly referenced from the exported file instead of linking back to Discord CDN. The files which are downloaded include: guild icons, user avatars, attachments, embedded images, reaction emojis. Note that only files which are actually referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using plain text export format. This option is not meant to enable complete offline viewing for HTML exports, but rather to make it easier to archive media content that may eventually get deleted from Discord servers. Also keep in mind that this option may make the export drastically slower and the total file size larger.
- Changed "discordapp.com" to "discord.com" where applicable as Discord is migrating to a new domain. CDN will remain on "cdn.discordapp.com" for the foreseeable future.

Note that all existing and current HTML exports will likely not render accurately because Discord enabled CORS for their font resources, which prevents them from loading locally. Please refer to [issue #322](https://github.com/Tyrrrz/DiscordChatExporter/issues/322) for discussion on this topic. 

### v2.20 (27-Apr-2020)

- [CLI] Added environment variables as fallback for `--token` and `--bot` options. They are `DISCORD_TOKEN` and `DISCORD_TOKEN_BOT` respectively.
- [GUI] Fixed a crash that occurred if the application was closed in the middle of an auto-update.
- [GUI] Added a link to project's wiki in the usage guide.
- [HTML] Added full spoiler support for text and images. To spoil the hidden content you now need to click on it, similarly to how it works in Discord. (Thanks [@FM-96](https://github.com/FM-96))
- [HTML] Updated grammar for consecutive single-line quotes. Previously they appeared separately, now they're grouped together as if it was one multi-line quote instead.
- [HTML] Fixed an issue where messages sent by the same bot but with different usernames were grouped together when they shouldn't be.
- [HTML] Updated grammar for single-line code blocks to accept double backticks as synonymous with single backticks. This change makes code blocks appear the same, whether they were inserted manually or using Discord's buggy markdown preview mode.
- [HTML] Fixed styling inconsistencies and other minor issues.
- [TXT/JSON] Fixed an issue where markdown was not parsed correctly in embed fields.
- [HTML] Changed markdown formatting in embeds so it follows the same set of rules as in Discord.
- Added channel category to the default file name used when exporting chat logs.
- [HTML/TXT/JSON] Added channel category to exported metadata.
- [CLI] Updated commands to also show channel category as well as channel name wherever relevant.
- Starting with this release, Docker images will have new tags: `stable` for latest stable release and `2.20` for a specific version.

### v2.19 (26-Mar-2020)

- Added parallel exporting. This option allows you to export multiple channels faster by doing it in parallel. You can configure the parallel limit in settings (GUI) or with the `--parallel` option (CLI). Default value is `1`, which means there is no parallelization. Warning: be careful when using this option, as this will result in higher volume of concurrent HTTP requests sent to Discord, which might get you flagged. Use at your own risk.
- [HTML] Added support for user colors. User names are now appropriately colored according to the assigned guild roles.
- [HTML] Added support for nicknames. User names are replaced with nicknames where it's applicable. It's still possible to see the full user name by hovering your mouse over the nickname.
- [HTML] Improved styling for mentions to bring it more in line with how it looks in Discord.

### v2.18 (04-Feb-2020)

- Added JSON export format. It's a structured data format which is easy to parse. If you're using DiscordChatExporter to export chat logs for further ingestion by another tool, this is most likely the format you will want to use.
- [GUI] You can now quickly jump to a specific channel in a list. For example, if you want to jump to a channel named "General", you can simply press the 'g' key in the channel list view. You can also press 'g', 'e' and 'n' in quick succession which will jump to the next channel that starts with "gen". This mechanic is consistent with other Windows applications.
- [CLI] Improved help text screen.

### v2.17 (12-Jan-2020)

- Fixed an issue where an empty file was produced when exporting a channel with no messages (for specified period). With the new behavior, no file will be created and instead a message will be shown to the user informing of the failure.
- [HTML/TXT] Added message count to the bottom of the file. This number shows how many messages were exported as part of the current file. Prior to 2.16 this number was present along with the rest of the metadata at the top, then removed due to changes in the exporter, and now brought back at the end of the file instead.
- [HTML] Fixed an issue where some emojis were not properly rendered.
- [GUI] Added a setting that lets you configure whether to remember the last used token. Previously it was the default behavior, but now you can disable it if you don't want the token persisted on your system.

### v2.16 (08-Dec-2019)

- Migrated from .NET Framework to .NET Core. To run this and future versions of DiscordChatExporter you will need [.NET Core runtime for desktop apps](https://dotnet.microsoft.com/download/dotnet-core/3.1/runtime) (for GUI/CLI version on Windows) or [.NET Core base runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1) (for CLI version on Windows, Linux or macOS).
- Reworked the exporter engine to render output using streaming data source. This means that only a very small portion of messages are cached in memory when exporting. In other words, you are only limited by storage space and not how much RAM you have, so you can now export terabytes of chat logs without hitting out of memory exceptions. I still recommend using partitions if you're exporting very large chat logs (250k+ messages) to HTML, unless you want your computer to catch fire when you try to open the export in browser.
- Changed how partitioned files are named, due to the fact that total number of partitions is no longer known ahead of time.
- Added a warning about automating user accounts to the usage guide in both GUI and CLI versions.
- Added support for announcement (news) channels. You will now be able to see them in the list and export them.
- Fixed various issues that resulted in exceptions during export process.
- [HTML/TXT] Removed message count from the metadata. Due to the fact that the messages are exported as they are streamed, total number of messages is not known ahead of time.
- [HTML] Changed default color of the embed color pill to match the theme.
- [HTML] Changed emoji parser to be less greedy. As a result it should match fewer character sequences that look like emojis but really aren't, but on the other hand it might miss some actual emojis. This means that some standard emojis (i.e. not custom server emojis) may not look like in Discord. This is a compromise I'm willing to take because detecting emojis in text is really hard to get right and not worth it at all.
- [HTML] Some other minor styling adjustments.
- [CSV] Replaced semicolons (;) with (,) as separators.

Please note that CSV export is most likely not going to receive future updates and will remain only as an option for those who wish to use it for text analysis. In other words, it won't contain any of the rich media features like embeds. If you're currently using CSV as a structured output to feed into your program, JSON output will replace that in the future.

### v2.15 (15-Sep-2019)

- Improved markdown parser and made it even faster for non-HTML formats.
- [HTML] Added support for block quotes.
- [HTML] Links pointing to a Discord message will now navigate to the linked message inside exported chat log if it's there.
- [HTML] Updated light theme to match how it looks in Discord after recent changes.
- [HTML] Added indication for when a message is pinned. Pinned messages now have a tinted background.
- [HTML] Fixed an issue where multiline code blocks sometimes had incorrect formatting applied to them.
- [TXT] Added indication for when a message is pinned. Pinned messages now have `(pinned)` next to timestamp and author.
- [CSV] Added message author's user ID to output.
- [GUI] Streamlined auto-update process a bit.
- [GUI] Added some tooltips.

### v2.14 (15-Jun-2019)

- [TXT] Added support for embeds.
- [TXT] Added support for reactions.
- [CSV] Added support for reactions.
- [TXT] Changed how attachments are rendered.

### v2.13.1 (06-Jun-2019)

- Fixed an issue where the app sometimes crashed when exporting due to `System.InvalidCastException`.
- [HTML] Added `data-user-id` attribute to `span.chatlog__author-name`. The value of this attribute is author's Discord user ID.

### v2.13 (15-May-2019)

- Updated usage instructions.
- [HTML] Added BOT tags to messages posted by bots.
- [GUI] UI will now be disabled when exporting.
- [GUI] Notification informing that the export has completed will now appear after all exports have completed, instead of each of them separately.

### v2.12.1 (12-Apr-2019)

- [GUI] Fixed an issue where the app crashed when trying to export multiple channels.
- [HTML] Fixed an issue where some of the emojis were rendered via Twemoji while Discord renders them as plain text.
- [HTML] Fixed an issue where emojis weren't jumbo if there were more than one of them.

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