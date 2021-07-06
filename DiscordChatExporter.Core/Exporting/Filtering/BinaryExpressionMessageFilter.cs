using DiscordChatExporter.Core.Discord.Data;

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

        public override bool Filter(Message message)
        {
            var first = _first.Filter(message);
            var second = _second.Filter(message);
            return _kind == BinaryExpressionKind.Or ?
                first || second :
                first && second;
        }
    }
}
