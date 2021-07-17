using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    internal class NullMessageFilter : MessageFilter
    {
        public override bool Filter(Message message) => true;
    }
}