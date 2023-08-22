using System;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class ReactionMessageFilter : MessageFilter
{
    private readonly string _value;

    public ReactionMessageFilter(string value) => _value = value;

    public override bool IsMatch(Message message) =>
        message.Reactions.Any(
            r =>
                string.Equals(_value, r.Emoji.Id?.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(_value, r.Emoji.Name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(_value, r.Emoji.Code, StringComparison.OrdinalIgnoreCase)
        );
}
