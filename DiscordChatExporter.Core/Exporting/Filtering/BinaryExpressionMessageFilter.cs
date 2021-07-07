using DiscordChatExporter.Core.Discord.Data;
using System;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class BinaryExpressionMessageFilter : MessageFilter
    {
        private readonly MessageFilter _first;
        private readonly MessageFilter _second;
        private readonly BinaryExpressionKind _kind;

        public BinaryExpressionMessageFilter(MessageFilter first, MessageFilter second, BinaryExpressionKind kind)
        {
            _first = first;
            _second = second;
            _kind = kind;
        }

        public override bool Filter(Message message) => _kind switch
        {
            BinaryExpressionKind.Or => _first.Filter(message) || _second.Filter(message),
            BinaryExpressionKind.And => _first.Filter(message) && _second.Filter(message), 
            _ => throw new InvalidOperationException($"Unknown binary expression kind '{_kind}'.")
        };
    }
}
