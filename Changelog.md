### v2.36.1 (24-Sep-2022)

- Added a check which will trigger an error if the provided bot account does not have the message content intent enabled. Note, however, that this check is based on heuristics which may result in false negatives.
- Fixed an issue where certain transient HTTP errors were not retried.
- Fixed an issue which caused the export process to fail with the `IndexOutOfRangeException` exception on certain automated messages.
- Fixed an issue which caused the export process to fail on unrecognized embed types.

### v2.36 (16-Sep-2022)

- [HTML] Added support for rendering GIFV embeds. They will now render as videos that automatically play when you hover your mouse over them. (Thanks [@gan-of-culture](https://github.com/gan-of-culture))
- [HTML] Added support for rendering system notification messages, such as when a user joins a server or when a messages gets pinned. Previously, such messages were rendered as regular text messages, but now they look similar to how they're presented in the Discord client. (Thanks [@gan-of-culture](https://github.com/gan-of-culture))
- [CLI] Improved the wording of the error message that's shown when exporting multiple channels and the provided output path is ambiguous. It now mentions that you can resolve the ambiguity by adding a slash at the end of the path.

### v2.35.2 (08-Sep-2022)

- Updated usage guide to mention the need to enable "Message Content Intent" to use a bot as a medium for exporting messages.
- [CLI] Fixed an issue where passing an existing directory as the output path failed when exporting multiple channels (for example, when running `exportdm`). An ambiguous output path will now always be treated as a directory path if that directory already exists. If it doesn't exist, it will be treated as a file path, matching the previous behavior.

### v2.35.1 (26-Aug-2022)

- Added `~` character as an alias for `-` when negating a message filter. This should make it easier to use negated filters from CLI where the dash character already has other meanings.
- Extended retry logic to encompass all server-side errors from Discord API, instead of only `500 Internal Server Error`.
- [CLI] Attempt to export multiple channels will now fail if provided with an output path that is neither a directory, nor a file name template. Previously, this resulted in all channels being exported to the same file, overwriting each other.
- Fixed an issue where a text message filter didn't work if it started with a character that's also the first character in one of the named filters (e.g. `from` or `mentions`).
- Fixed an issue where a text message filter didn't work correctly if it was enclosed in parentheses.
- Fixed an issue where categories were displayed alongside regular channels in the channel list, both in GUI and in CLI (when using the `guild` command).
- [CLI] Fixed an issue where an output path starting with a tilde on Linux was not expanded properly into a full path. (Thanks [@TSRBerry](https://github.com/TSRBerry))

### v2.35 (02-Jul-2022)

- Added support for exporting text chat in voice channels. These channels are now also displayed when browsing in the GUI and in the CLI when executing the `channels` command.
- [GUI] Direct message channels are now sorted by the timestamp of their last message, similarly to how they appear in the Discord client.
- [GUI] Removed artificial grouping of direct message channels into "Private" and "Group".
- [CLI] Added runtime bootstrapper capabilities to the CLI flavor of DiscordChatExporter on Windows. Now, if you attempt to run the application without the required prerequisites installed, you will be provided with an option to install them automatically.
- [CLI] Fixed outdated information in the usage guide. (Thanks [@Clint Herron](https://github.com/HanClinto))
- [CLI] Added whitespace padding to the output of `guilds`, `channels`, and `dm` to make the tables look more uniform.
- [HTML] Added hyperlinks to message timestamps, which allows you to quickly copy a link to a specific message in the export. (Thanks [@Philipp C. Heckel](https://github.com/binwiederhier))
- [HTML] Added minification. Chat exports in HTML format are now 30-35% smaller than before.
- [HTML] Added support for rendering embeds of Twitter posts that contain multiple images. Previously, this resulted in multiple separate embeds instead of one.
- [HTML] Added support for rendering embeds of YouTube Shorts videos.
- [HTML] Text content is now hidden if the message only contains a link to an image and nothing else. The link itself is resolved as an image embed.
- [HTML] Fixed an issue where some emoji that included a zero-width-joiner rune were not rendered correctly due to a wrong Twemoji URL being generated. (Thanks [@Ethan](https://github.com/ethanndickson))
- [HTML] Fixed an issue where replies to a message that contained a quote (i.e. `>`) were not rendered correctly.
- [HTML] Fixed an issue where code blocks with language highlighting did not have the correct background color.
- Added file name template token that resolves to the current date (`%d`). (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- Updated the usage guide to replace the instructions for retrieving the user token with those that appear to work for more people.
- Updated the usage guide to feature the TOS warning more prominently.
- [Docker] Changed internal working directory from `/app/out` to `/out`. This was an unintended breaking change. Please update your volume binding configuration from `-v /path/on/machine:/app/out` to `-v /path/on/machine:/out`.

### v2.34.1 (01-Jun-2022)

- Improved retry policy to handle more transient errors. This should result in fewer errors exporting a channel due to timeout.
- Added `has:pin` message filter. You can use it to limit exported messages to those that have been pinned. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Fixed an issue which caused the export to fail when processing stickers that have no name.

### v2.34 (10-Apr-2022)

- [HTML] Reworked layout to be more consistent with Discord.
- [HTML] Changed user profile picture dimensions from `128x128` to `512x512`. (Thanks [@Chris Helder](https://github.com/TheDude53))
- [HTML] Improved image scaling for Chromium-based browsers. This should result in higher quality images across the entire export.
- [HTML] Hovering over messages will now show short timestamps on the left, indicating the time that the individual messages of the group were sent.
- [HTML] Using "download media" option will now also download scripts, styles, fonts, and other similar resources used by the export.
- [HTML] Image attachments will now use the provided description as alt text.
- [HTML] Fixed a few layout inconsistencies related to embeds.
- [GUI] Added a few clickable links to the usage guide to make the process of locating tokens easier.

### v2.33.2 (07-Mar-2022)

- Actually fixed it this time.

### v2.33.1 (07-Mar-2022)

- Fixed an issue where the application silently failed to run if the system didn't have .NET Runtime 6.0.2 installed. If you continue seeing this issue, please uninstall all existing .NET runtimes from your computer and then try running the application again.

### v2.33 (06-Mar-2022)

- Added messages informing about war in Ukraine and available ways to help.
- Added support for rendering stickers in HTML and JSON. Lottie-based stickers currently cannot be displayed in HTML exports (see [#803](https://github.com/Tyrrrz/DiscordChatExporter/issues/803)).
- Added a new `reaction:` message filter which can be used to check if someone reacted to a message with a specific emoji. You can either pass the emoji name (e.g. `reaction:LUL`) or its ID (e.g. `reaction:41771983429993937`).
- [GUI] Added auto-detection for dark mode. If your system is configured to prefer dark mode in applications, DiscordChatExporter will use it by default instead of light mode.
- Fixed an issue which caused the export process to crash when downloading media files with extremely long file extensions. (Thanks [@Tomlacko](https://github.com/Tomlacko))
- Fixed an issue which caused the export process to crash on invalid mentions.
- [GUI] Fixed an issue where the time pickers used to specify export ranges always displayed time in 12-hour format, instead of respecting the system locale.

### v2.32 (27-Jan-2022)

- Token kind (user or bot) is now detected automatically. Removed the button to switch token kind in GUI. Option `-b|--bot` in CLI is now deprecated and does nothing.
- Updated user token extraction guide to reflect the fact that devtools are no longer accessible in the desktop version of Discord client. The recommended workaround is to open Discord in browser. (Thanks [@Dhananjay-JSR](https://github.com/Dhananjay-JSR))
- Fixed an issue which caused an error when parsing invalid date timestamps. Such timestamps are now rendered as "Invalid date", similarly to how the Discord client does it.
- Fixed an issue which caused an error when parsing certain embed fields.
- Fixed an issue which caused an error when parsing a mention to a user that has no username.
- [GUI] Minor visual fixes.

### v2.31.1 (15-Dec-2021)

- Fixed an issue which caused an error when parsing certain custom emoji reactions.
- [GUI] Improved user interface.

### v2.31 (06-Dec-2021)

- Changed target runtime of the application (both GUI and CLI) from .NET 3.1 to .NET 6. Those using the GUI version will not need to take any action as the application should install the necessary prerequisites automatically. Those using the CLI version will need to download and install .NET 6 runtime manually [from here](https://dotnet.microsoft.com/download/dotnet/6.0) (look for ".NET Runtime 6.0.x" section on the right and choose the distribution appropriate for your system).
- [HTML] Updated layout design to match Discord more closely. (Thanks [@Artem](https://github.com/Scarg))
- [HTML] Fixed an issue where markdown inside channel topic was not rendered correctly. (Thanks [@Emily](https://github.com/emillly-b))
- [HTML] Fixed an issue where the layout was broken for messages containing text in right-to-left languages. (Thanks [@Emily](https://github.com/emillly-b))
- Fixed an issue where text filters weren't matching content inside of embeds.
- [GUI] Fixed a multitude of issues related to the bootstrapper.

### v2.30.1 (28-Aug-2021)

- [HTML] Changed user avatars to render using higher resolution images. (Thanks [@Chris Helder](https://github.com/TheDude53))
- Fixed an issue where `has:file` message filter was not recognized. (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- Fixed an issue where a unix timestamp marker with an invalid offset crashed the export process.

### v2.30 (08-Aug-2021)

- [HTML] Added special casing for Spotify track embeds to render them directly using an embedded player.
- [HTML] Added special casing for plain image embeds to render them like image attachments.
- [HTML] Added tooltip text for unix timestamp markers which shows the full date represented by the timestamp, regardless of configured date format.
- [HTML] Updated the fallback text shown when the referenced message could not be loaded to make it more explicit.
- [HTML] Updated styling of quoted text in markdown to more closely resemble how it looks in Discord.
- [HTML] Fixed an issue where some file formats were not correctly recognized as image or video attachments, which affected how they were rendered.
- [HTML] Fixed an issue where formatting inside named links (e.g. `[**Bold link**](https://example.com)`) was not being parsed.
- [HTML] Fixed an issue where inline embed fields were missing horizontal spacing between columns.
- [HTML] Fixed other minor styling issues.

### v2.29 (19-Jul-2021)

- Added support for message filtering, which is accessible as an additional export parameter in the GUI and as the `--filter` option in the CLI. Message filters use a special syntax similar to Discord's search query syntax to allow specifying which messages should be included in the export. For example, you can limit the export to only include messages that contain image attachments (`has:image`) or messages from a certain user (`from:Tyrrrz`) or messages containing certain words (`hello world`), etc. You can also combine filters together (`hi mentions:Tyrrrz`, `from:Tyrrrz | from:96-LB`, `(mentions:Tyrrrz | from:Tyrrrz) has:image`) or negate them (`-has:link`). More thorough documentation coming soon. (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- Added support for rendering mentions of voice channels, in addition to text channels.
- Added support for rendering unix timestamp markers. Currently, style markers are ignored and timestamps are always rendered with the format configured in settings (or provided via `--date-format` option).
- Updated in-app instructions for obtaining a user token. (Thanks [@Yudi](https://github.com/Yudi) and [@virtuallyvlad](https://github.com/virtuallyvlad))
- [HTML] Improved visual appearance of the reference symbol (Г-shaped line in replies) to make it more closely resemble Discord's styling.
- [HTML] Added more specific fallback text used when rendering referenced messages that contain no text content but have attachments or embeds.
- [HTML] Fixed an issue where a broken embed author icon occupied visual space, ruining the embed layout. (Thanks [@Cliff J.](https://github.com/CliffJameston))
- [GUI] Improved handling for fatal errors that may occur when attempting to export a channel. These errors are now displayed in a popup, allowing the user to try again instead of having the application crash completely.
- [CLI] Fixed an issue where an attempt to export a channel resulted in `NotSupportedException` when the default encoding was set to `UTF8`.
- [CLI] Fixed an issue where providing an incorrectly formatted partition limit via `-p|--partition` option resulted in a non-descriptive and unhelpful error.
- Improved "Requested resource does not exist" error message by including the ID of the requested resource.

### v2.28 (19-Jun-2021)

- [HTML] Added special casing for YouTube video embeds to render them directly using an embedded player. (Thanks [@quentinmay](https://github.com/quentinmay))
- [HTML] Added support for rendering standard emoji by code. Such emoji may sometimes be found in messages sent by bots or through webhooks. (Thanks [@CanePlayz](https://github.com/CanePlayz) and [@Lucas LaBuff](https://github.com/96-LB))
- [HTML] Changed tooltips on standard emoji to show emoji code instead of their raw string representations. (Thanks [@CanePlayz](https://github.com/CanePlayz) and [@Lucas LaBuff](https://github.com/96-LB))
- [HTML] Added tooltips on individual messages to show when those messages were sent.
- [HTML] Updated colors and minor styling elements to match Discord's new style direction after rebranding.
- [HTML] Updated Twemoji URLs to use SVG image variants for better rendering quality.
- [HTML] Changed user avatar URLs to include predetermined size query parameter for better rendering quality.
- Changed embed rendering logic to prefer Discord-proxied URLs for external content, in order to avoid unnecessary HTTP requests to third parties.
- Fixed an issue where exporting with media sometimes resulted in a crash due to lack of permissions to change file attributes. These errors are now ignored, which means that the creation, write, and access dates of downloaded files may sometimes not match the dates provided by Discord's CDN.
- [HTML] Fixed styling issues when rendering embed footers.
- [HTML] Fixed an issue where links didn't render properly inside referenced messages.
- [HTML] Fixed an issue where standalone emoji were incorrectly enlarged inside referenced messages.
- [HTML] Fixed an issue where clicking on a referenced message revealed spoilers inside of it.
- [GUI] Fixed an issue where the list of missing components, reported by the prerequisite check at application startup, included Windows updates that were not applicable to the user's system due to the fact that other superseding updates have already been installed. This issue only affected users running DiscordChatExporter on Windows 7.
- [GUI] Fixed a few other minor issues related to the prerequisite installation window.

### v2.27.1 (08-Jun-2021)

- [GUI] Application will now detect if the required .NET Runtime or any of its prerequisites are missing and prompt the user to download and install them automatically. **Experimental feature, please test it out and report any issues you may find!**
- [CLI] Fixed an issue where the application crashed when exporting a channel that had square brackets as part of its name or category name. (Thanks [@Lucas LaBuff](https://github.com/96-LB))

### v2.27 (24-Apr-2021)

- Added partitioning by file size. You can now use values such as `10mb` to indicate a size-based cut off point, in addition to values like `10` to indicate a number of messages. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- [CLI] Improved formatting and progress reporting.
- [CLI] Changed `export` command so that it can accept multiple channel IDs passed to `-c|--channel` option. This makes it possible to export multiple channels at once while benefiting from parallelization.
- [JSON] Added `color` field to message author. This is a hex string that represents user's color, inherited from their roles. (Thanks [@rtm516](https://github.com/rtm516))
- [JSON] Added `categoryId` field to the root. This represents the ID of the category channel that the currently exported channel belongs to. (Thanks [@rtm516](https://github.com/rtm516))
- [JSON] Fixed an issue where user discriminator was written without leading zeroes. (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- [JSON] Fixed an issue where the application sometimes crashed with an error saying `']' invalid without a matching open`, which was just a validation message that masked the actual error behind the failure. (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- Fixed an issue where progress was not reported correctly if the channel didn't have any messages in the specified time range.

### v2.26.1 (06-Feb-2021)

- [CLI] Added support for file name templates, which allow you to dynamically generate output file names based on channel and guild metadata. (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- Fixed an issue where the application sometimes crashed with `Invalid snowflake` message when exporting chat logs that contained invalid or outdated mentions. (Thanks [@Lucas LaBuff](https://github.com/96-LB))

### v2.26 (29-Dec-2020)

- [HTML] Added support for replies. (Thanks [@Sanqui](https://github.com/Sanqui))
- [HTML] Improved styling of non-media attachments (regular files, i.e. not video, audio, or image). (Thanks [@Lucas LaBuff](https://github.com/96-LB))
- [HTML] Improved initial export performance.
- [JSON] Relaxed escaping rules for unicode characters. (Thanks [@Sanqui](https://github.com/Sanqui))
- [JSON] Added support for replies. (Thanks [@Sanqui](https://github.com/Sanqui))
- [CLI] Added channel names to errors shown on failures when exporting multiple channels. (Thanks [@Sanqui](https://github.com/Sanqui))
- [CLI] Added support for passing message IDs (snowflakes) to `--after` and `--before` options. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Files downloaded by the "download referenced media" option now have their dates set according to the `Last-Modified` header provided by Discord CDN. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Fixed an issue where the application sometimes crashed with `NullReferenceException` when retrying a failed HTTP request.
- Migrated to Discord API v8.

### v2.25 (23-Nov-2020)

- [JSON] Added `mentions` property to the output. This contains users referenced inside the message text, but doesn't include mentions in embeds. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- [HTML] Added better previews for attached video and audio. They are now rendered using a browser native player element. The actual file path can still be retrieved by right clicking and selecting "Copy file path". (Thanks [@sas41](https://github.com/sas41))
- [CLI] Fixed an issue where using `--media` option without providing a rooted path via `--output` option triggered an error.
- [CLI] Fixed an issue where using `--reuse-media` option without also providing `--media` option was accepted, but didn't actually do anything. Now this produces an error instead. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))

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

- Added a new option that enables self-contained exports for all output formats. You can turn it on in the export setup dialog in GUI or using the `--media` option in CLI. When using this, the application will additionally download any media content directly referenced from the exported file instead of linking back to Discord CDN. The files which are downloaded include: guild icons, user avatars, attachments, embedded images, reaction emoji. Note that only files which are actually referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using plain text export format. This option is not meant to enable complete offline viewing for HTML exports, but rather to make it easier to archive media content that may eventually get deleted from Discord servers. Also keep in mind that this option may make the export drastically slower and the total file size larger.
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
- [HTML] Fixed an issue where some emoji were not properly rendered.
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
- [HTML] Changed emoji parser to be less greedy. As a result it should match fewer character sequences that look like emoji but really aren't, but on the other hand it might miss some actual emoji. This means that some standard emoji (i.e. not custom server emoji) may not look like in Discord. This is a compromise I'm willing to take because detecting emoji in text is really hard to get right and not worth it at all.
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
- [HTML] Fixed an issue where some of the emoji were rendered via Twemoji while Discord renders them as plain text.
- [HTML] Fixed an issue where emoji weren't jumbo if there were more than one of them.

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

- [HTML] Fixed an issue where multiple emoji on a single line would get rendered as one emoji.
- [HTML] Fixed an issue where an open square bracket character would incorrectly render as an emoji.
- [HTML] Added alt text to emoji images so that they will be rendered using default font when Twemoji images are not available.

### v2.10 (03-Mar-2019)

- [HTML] Reworked the entire markdown parsing engine which fixes numerous rendering inconsistencies (over 15 issues fixed).
- [HTML] Added support for animated emoji.
- [HTML] Standard emoji are now rendered using Twemoji, the same image set that Discord uses.
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
- Added support for jumbo emoji (custom emoji only).
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
- Added support for custom emoji.
- Added support for user and role mentions.
- Added support for channel mentions.
- Fixed text in pre blocks not being wrapped correctly.
- Added workaround for non-default message types.