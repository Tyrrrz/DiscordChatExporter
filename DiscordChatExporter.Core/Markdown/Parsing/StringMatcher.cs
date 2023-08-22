using System;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class StringMatcher<T> : IMatcher<T>
{
    private readonly string _needle;
    private readonly StringComparison _comparison;
    private readonly Func<StringSegment, T?> _transform;

    public StringMatcher(
        string needle,
        StringComparison comparison,
        Func<StringSegment, T?> transform
    )
    {
        _needle = needle;
        _comparison = comparison;
        _transform = transform;
    }

    public StringMatcher(string needle, Func<StringSegment, T> transform)
        : this(needle, StringComparison.Ordinal, transform) { }

    public ParsedMatch<T>? TryMatch(StringSegment segment)
    {
        var index = segment.Source.IndexOf(
            _needle,
            segment.StartIndex,
            segment.Length,
            _comparison
        );
        if (index < 0)
            return null;

        var segmentMatch = segment.Relocate(index, _needle.Length);
        var value = _transform(segmentMatch);

        return value is not null ? new ParsedMatch<T>(segmentMatch, value) : null;
    }
}
