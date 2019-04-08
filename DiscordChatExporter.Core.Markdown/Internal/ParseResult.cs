using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class ParseResult<T>
    {
        public T Value { get; }

        public int StartIndex { get; }

        public int Length { get; }

        public ParseResult(T value, int startIndex, int length)
        {
            Value = value;
            StartIndex = startIndex;
            Length = length;
        }
    }
}