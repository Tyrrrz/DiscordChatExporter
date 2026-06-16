using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Filtering.Parsing;
using Superpower;

namespace DiscordChatExporter.Core.Exporting.Filtering;

public abstract partial class MessageFilter
{
    public abstract bool IsMatch(Message message);
}

public partial class MessageFilter
{
    public static MessageFilter Null { get; } = new NullMessageFilter();

    public static MessageFilter Parse(string value) => FilterGrammar.Filter.Parse(value);
}
