using System;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class MentionsMessageFilter : MessageFilter
{
    private readonly string _value;

    public MentionsMessageFilter(string value) => _value = value;

    public override bool IsMatch(Message message) =>
        message
            .MentionedUsers
            .Any(
                user =>
                    string.Equals(_value, user.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_value, user.DisplayName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_value, user.FullName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_value, user.Id.ToString(), StringComparison.OrdinalIgnoreCase)
            );
}
