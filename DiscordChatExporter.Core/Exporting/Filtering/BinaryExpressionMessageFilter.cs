using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class BinaryExpressionMessageFilter : MessageFilter
{
    private readonly MessageFilter _first;
    private readonly MessageFilter _second;
    private readonly BinaryExpressionKind _kind;

    public BinaryExpressionMessageFilter(
        MessageFilter first,
        MessageFilter second,
        BinaryExpressionKind kind
    )
    {
        _first = first;
        _second = second;
        _kind = kind;
    }

    public override bool IsMatch(Message message) =>
        _kind switch
        {
            BinaryExpressionKind.Or => _first.IsMatch(message) || _second.IsMatch(message),
            BinaryExpressionKind.And => _first.IsMatch(message) && _second.IsMatch(message),
            _ => throw new InvalidOperationException($"Unknown binary expression kind '{_kind}'.")
        };
}
