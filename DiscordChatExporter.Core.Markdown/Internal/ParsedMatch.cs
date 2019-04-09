namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal partial class ParsedMatch<T>
    {
        public int StartIndex { get; }

        public int Length { get; }

        public T Value { get; }

        public ParsedMatch(int startIndex, int length, T value)
        {
            StartIndex = startIndex;
            Length = length;
            Value = value;
        }
    }
}