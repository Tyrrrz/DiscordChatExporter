using PowerArgs;

namespace DiscordChatExporter.Cli.Options
{
    public class GetGuildsOptions
    {
        [ArgShortcut("-t"), ArgShortcut("--token"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgRequired,
         ArgDescription("Authorization token")]
        public string TokenValue { get; set; }

        [ArgShortcut("-b"), ArgShortcut("--bot"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(false),
         ArgDescription("Whether the token is a bot token")]
        public bool IsBotToken { get; set; }
    }
}