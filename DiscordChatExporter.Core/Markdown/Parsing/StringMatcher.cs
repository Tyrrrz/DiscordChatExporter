using System;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class StringMatcher<T>(
    string needle,
    StringComparison comparison,
    Func<StringSegment, T?> transform
) : IMatcher<T>
{
    public StringMatcher(string needle, Func<StringSegment, T> transform)
        : this(needle, StringComparison.Ordinal, transform) { }

    public ParsedMatch<T>? TryMatch(StringSegment segment)
    {
        var index = segment.Source.IndexOf(needle, segment.StartIndex, segment.Length, comparison);

        if (index < 0)
            return null;

        var segmentMatch = segment.Relocate(index, needle.Length);
        var value = transform(segmentMatch);

        return value is not null ? new ParsedMatch<T>(segmentMatch, value) : null;
    }
}
