using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class FromMessageFilter(string value) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        string.Equals(value, message.Author.Name, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, message.Author.DisplayName, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, message.Author.FullName, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, message.Author.Id.ToString(), StringComparison.OrdinalIgnoreCase);
}
