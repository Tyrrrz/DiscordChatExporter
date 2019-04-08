using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    public class TransformedMatch<T>
    {
        public Match Match { get; }

        public T Value { get; }

        public TransformedMatch(Match match, T value)
        {
            Match = match;
            Value = value;
        }
    }
}