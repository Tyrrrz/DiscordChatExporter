using PowerArgs;

namespace DiscordChatExporter.Cli.Options
{
    public class GetChannelsOptions
    {
        [ArgShortcut("-t"), ArgShortcut("--token"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgRequired,
         ArgDescription("Authorization token")]
        public string TokenValue { get; set; }

        [ArgShortcut("-b"), ArgShortcut("--bot"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(false),
         ArgDescription("Whether the token is a bot token")]
        public bool IsBotToken { get; set; }

        [ArgShortcut("-g"), ArgShortcut("--guild"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgRequired,
         ArgDescription("Guild ID")]
        public string GuildId { get; set; }
    }
}