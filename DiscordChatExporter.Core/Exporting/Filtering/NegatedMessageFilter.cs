using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    internal class NegatedMessageFilter : MessageFilter
    {
        private readonly MessageFilter _filter;

        public NegatedMessageFilter(MessageFilter filter) => _filter = filter;

        public override bool Filter(Message message) => !_filter.Filter(message);
    }
}