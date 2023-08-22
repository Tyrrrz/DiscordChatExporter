namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class ParsedMatch<T>
{
    public StringSegment Segment { get; }

    public T Value { get; }

    public ParsedMatch(StringSegment segment, T value)
    {
        Segment = segment;
        Value = value;
    }
}
