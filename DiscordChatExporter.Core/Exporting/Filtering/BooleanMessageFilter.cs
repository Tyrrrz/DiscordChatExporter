using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class BooleanMessageFilter : MessageFilter
    {
        private readonly MessageFilter _first;
        private readonly MessageFilter _second;
        private readonly bool _unioned;

        public BooleanMessageFilter(MessageFilter first, MessageFilter second, bool unioned)
        {
            _first = first;
            _second = second;
            _unioned = unioned;
        }

        public override bool Filter(Message message)
        {
            var first = _first.Filter(message);
            var second = _second.Filter(message);
            return _unioned ?
                first || second :
                first && second;
        }
    }
}
