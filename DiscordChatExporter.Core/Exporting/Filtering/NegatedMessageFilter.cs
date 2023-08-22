using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class NegatedMessageFilter : MessageFilter
{
    private readonly MessageFilter _filter;

    public NegatedMessageFilter(MessageFilter filter) => _filter = filter;

    public override bool IsMatch(Message message) => !_filter.IsMatch(message);
}
