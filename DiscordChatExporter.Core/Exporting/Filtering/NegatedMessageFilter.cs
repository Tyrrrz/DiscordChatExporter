using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class NegatedMessageFilter(MessageFilter filter) : MessageFilter
{
    public override bool IsMatch(Message message) => !filter.IsMatch(message);
}
