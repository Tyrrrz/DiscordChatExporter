using System;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class MentionsMessageFilter(string value) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        message.MentionedUsers.Any(user =>
            string.Equals(value, user.Name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, user.DisplayName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, user.FullName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, user.Id.ToString(), StringComparison.OrdinalIgnoreCase)
        );
}
