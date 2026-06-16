namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class ParsedMatch<T>(StringSegment segment, T value)
{
    public StringSegment Segment { get; } = segment;

    public T Value { get; } = value;
}
