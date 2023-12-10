using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class BinaryExpressionMessageFilter(
    MessageFilter first,
    MessageFilter second,
    BinaryExpressionKind kind
) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        kind switch
        {
            BinaryExpressionKind.Or => first.IsMatch(message) || second.IsMatch(message),
            BinaryExpressionKind.And => first.IsMatch(message) && second.IsMatch(message),
            _ => throw new InvalidOperationException($"Unknown binary expression kind '{kind}'.")
        };
}
