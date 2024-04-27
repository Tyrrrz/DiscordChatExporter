using System;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class ReactionMessageFilter(string value) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        message.Reactions.Any(r =>
            string.Equals(value, r.Emoji.Id?.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, r.Emoji.Name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, r.Emoji.Code, StringComparison.OrdinalIgnoreCase)
        );
}
