using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class FromMessageFilter : MessageFilter
{
    private readonly string _value;

    public FromMessageFilter(string value) => _value = value;

    public override bool IsMatch(Message message) =>
        string.Equals(_value, message.Author.Name, StringComparison.OrdinalIgnoreCase)
        || string.Equals(_value, message.Author.DisplayName, StringComparison.OrdinalIgnoreCase)
        || string.Equals(_value, message.Author.FullName, StringComparison.OrdinalIgnoreCase)
        || string.Equals(_value, message.Author.Id.ToString(), StringComparison.OrdinalIgnoreCase);
}
