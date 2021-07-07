using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class NullMessageFilter : MessageFilter
    {
        public static NullMessageFilter Instance { get; } = new();

        public override bool Filter(Message message) => true;
    }
}