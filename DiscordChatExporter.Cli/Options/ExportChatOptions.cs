using System;
using DiscordChatExporter.Core.Models;
using PowerArgs;

namespace DiscordChatExporter.Cli.Options
{
    public class ExportChatOptions
    {
        [ArgShortcut("-t"), ArgShortcut("--token"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgRequired,
         ArgDescription("Authorization token")]
        public string TokenValue { get; set; }

        [ArgShortcut("-b"), ArgShortcut("--bot"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(false),
         ArgDescription("Whether the token is a bot token")]
        public bool IsBotToken { get; set; }

        [ArgShortcut("-c"), ArgShortcut("--channel"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgRequired,
         ArgDescription("Channel ID")]
        public string ChannelId { get; set; }

        [ArgShortcut("-f"), ArgShortcut("--format"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly),
         ArgDefaultValue(ExportFormat.HtmlDark), ArgDescription("Export format")]
        public ExportFormat ExportFormat { get; set; }

        [ArgShortcut("-o"), ArgShortcut("--output"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly),
         ArgDefaultValue(null), ArgDescription("Output file path")]
        public string FilePath { get; set; }

        [ArgShortcut("--from"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(null),
         ArgDescription("Only messages sent after this date")]
        public DateTime? From { get; set; }

        [ArgShortcut("--to"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(null),
         ArgDescription("Only messages sent before this date")]
        public DateTime? To { get; set; }

        [ArgShortcut("--dateformat"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(null),
         ArgDescription("Date format used in the output")]
        public string DateFormat { get; set; }

        [ArgShortcut("--grouplimit"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly), ArgDefaultValue(-1),
         ArgDescription("Maximum number of messages in one group")]
        public int MessageGroupLimit { get; set; }
    }
}