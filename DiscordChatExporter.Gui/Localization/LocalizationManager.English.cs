using System.Collections.Generic;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> EnglishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(PullGuildsTooltip)] = "Pull available servers and channels (Enter)",
            [nameof(SettingsTooltip)] = "Settings",
            [nameof(LastMessageSentTooltip)] = "Last message sent:",
            [nameof(TokenWatermark)] = "Token",
            // Token instructions (personal account)
            [nameof(TokenPersonalHeader)] = "To get the token for your personal account:",
            [nameof(TokenPersonalTosWarning)] =
                "*  Automating user accounts is technically against TOS — **use at your own risk**!",
            [nameof(TokenPersonalInstructions)] = """
                1. Open Discord in your web browser and login
                2. Open any server or direct message channel
                3. Press **Ctrl+Shift+I** to show developer tools
                4. Navigate to the **Network** tab
                5. Press **Ctrl+R** to reload
                6. Switch between random channels to trigger network requests
                7. Search for a request that starts with **messages**
                8. Select the **Headers** tab on the right
                9. Scroll down to the **Request Headers** section
                10. Copy the value of the **authorization** header
                """,
            // Token instructions (bot)
            [nameof(TokenBotHeader)] = "To get the token for your bot:",
            [nameof(TokenBotInstructions)] = """
                The token is generated during bot creation. If you lost it, generate a new one:

                1. Open Discord [developer portal](https://discord.com/developers/applications)
                2. Open your application's settings
                3. Navigate to the **Bot** section on the left
                4. Under **Token** click **Reset Token**
                5. Click **Yes, do it!** and authenticate to confirm
                *  Integrations using the previous token will stop working until updated
                *  Your bot needs to have the **Message Content Intent** enabled to read messages
                """,
            [nameof(TokenHelpText)] =
                "If you have questions or issues, please refer to the [documentation](https://github.com/Tyrrrz/DiscordChatExporter/tree/master/.docs)",
            // Settings
            [nameof(SettingsTitle)] = "Settings",
            [nameof(ThemeLabel)] = "Theme",
            [nameof(ThemeTooltip)] = "Preferred user interface theme",
            [nameof(LanguageLabel)] = "Language",
            [nameof(LanguageTooltip)] = "Preferred user interface language",
            [nameof(AutoUpdateLabel)] = "Auto-update",
            [nameof(AutoUpdateTooltip)] = "Perform automatic updates on every launch",
            [nameof(PersistTokenLabel)] = "Persist token",
            [nameof(PersistTokenTooltip)] =
                "Save the last used token to a file so that it can be persisted between sessions",
            [nameof(RateLimitPreferenceLabel)] = "Rate limit preference",
            [nameof(RateLimitPreferenceTooltip)] =
                "Whether to respect advisory rate limits. If disabled, only hard rate limits (i.e. 429 responses) will be respected.",
            [nameof(ShowThreadsLabel)] = "Show threads",
            [nameof(ShowThreadsTooltip)] = "Which types of threads to show in the channel list",
            [nameof(LocaleLabel)] = "Locale",
            [nameof(LocaleTooltip)] = "Locale to use when formatting dates and numbers",
            [nameof(NormalizeToUtcLabel)] = "Normalize to UTC",
            [nameof(NormalizeToUtcTooltip)] = "Normalize all timestamps to UTC+0",
            [nameof(ParallelLimitLabel)] = "Parallel limit",
            [nameof(ParallelLimitTooltip)] = "How many channels can be exported at the same time",
            // Export Setup
            [nameof(ChannelsSelectedText)] = "channels selected",
            [nameof(OutputPathLabel)] = "Output path",
            [nameof(OutputPathTooltip)] = """
                Output file or directory path.

                If a directory is specified, file names will be generated automatically based on the channel names and export parameters.

                Directory paths must end with a slash to avoid ambiguity.

                Available template tokens:
                - **%g** — server ID
                - **%G** — server name
                - **%t** — category ID
                - **%T** — category name
                - **%c** — channel ID
                - **%C** — channel name
                - **%p** — channel position
                - **%P** — category position
                - **%a** — after date
                - **%b** — before date
                - **%d** — current date
                """,
            [nameof(FormatLabel)] = "Format",
            [nameof(FormatTooltip)] = "Export format",
            [nameof(AfterDateLabel)] = "After (date)",
            [nameof(AfterDateTooltip)] = "Only include messages sent after this date",
            [nameof(BeforeDateLabel)] = "Before (date)",
            [nameof(BeforeDateTooltip)] = "Only include messages sent before this date",
            [nameof(AfterTimeLabel)] = "After (time)",
            [nameof(AfterTimeTooltip)] = "Only include messages sent after this time",
            [nameof(BeforeTimeLabel)] = "Before (time)",
            [nameof(BeforeTimeTooltip)] = "Only include messages sent before this time",
            [nameof(PartitionLimitLabel)] = "Partition limit",
            [nameof(PartitionLimitTooltip)] =
                "Split the output into partitions, each limited to the specified number of messages (e.g. '100') or file size (e.g. '10mb')",
            [nameof(MessageFilterLabel)] = "Message filter",
            [nameof(MessageFilterTooltip)] =
                "Only include messages that satisfy this filter (e.g. 'from:foo#1234' or 'has:image'). See the documentation for more info.",
            [nameof(FormatMarkdownLabel)] = "Format markdown",
            [nameof(FormatMarkdownTooltip)] =
                "Process markdown, mentions, and other special tokens",
            [nameof(DownloadAssetsLabel)] = "Download assets",
            [nameof(DownloadAssetsTooltip)] =
                "Download assets referenced by the export (user avatars, attached files, embedded images, etc.)",
            [nameof(ReuseAssetsLabel)] = "Reuse assets",
            [nameof(ReuseAssetsTooltip)] =
                "Reuse previously downloaded assets to avoid redundant requests",
            [nameof(AssetsDirPathLabel)] = "Assets directory path",
            [nameof(AssetsDirPathTooltip)] =
                "Download assets to this directory. If not specified, the asset directory path will be derived from the output path.",
            [nameof(AdvancedOptionsTooltip)] = "Toggle advanced options",
            [nameof(ExportButton)] = "EXPORT",
            // Common buttons
            [nameof(CloseButton)] = "CLOSE",
            [nameof(CancelButton)] = "CANCEL",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Thank you for supporting Ukraine!",
            [nameof(UkraineSupportMessage)] = """
                As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

                Click LEARN MORE to find ways that you can help.
                """,
            [nameof(LearnMoreButton)] = "LEARN MORE",
            [nameof(UnstableBuildTitle)] = "Unstable build warning",
            [nameof(UnstableBuildMessage)] = """
                You're using a development build of {0}. These builds are not thoroughly tested and may contain bugs.

                Auto-updates are disabled for development builds.

                Click SEE RELEASES if you want to download a stable release instead.
                """,
            [nameof(SeeReleasesButton)] = "SEE RELEASES",
            [nameof(UpdateDownloadingMessage)] = "Downloading update to {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "Update has been downloaded and will be installed when you exit",
            [nameof(UpdateInstallNowButton)] = "INSTALL NOW",
            [nameof(UpdateFailedMessage)] = "Failed to perform application update",
            [nameof(ErrorPullingGuildsTitle)] = "Error pulling servers",
            [nameof(ErrorPullingChannelsTitle)] = "Error pulling channels",
            [nameof(ErrorExportingTitle)] = "Error exporting channel(s)",
            [nameof(SuccessfulExportMessage)] = "Successfully exported {0} channel(s)",
        };
}
