using System;
using CliFx.Exceptions;
using CliFx.Extensibility;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Cli.Commands.Converters;

internal class MessageOrderBindingConverter : BindingConverter<MessageOrder>
{
    public override MessageOrder Convert(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return MessageOrder.Chronological;

        if (Enum.TryParse<MessageOrder>(rawValue, true, out var result))
            return result;

        throw new CommandException(
            $"Invalid order value '{rawValue}'. "
                + $"Expected: {string.Join(", ", Enum.GetNames<MessageOrder>()).ToLowerInvariant()}."
        );
    }
}
