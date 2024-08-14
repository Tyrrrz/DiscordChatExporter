using System;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class StringMatcher<TContext, TValue>(
    string needle,
    StringComparison comparison,
    Func<TContext, StringSegment, TValue?> transform
) : IMatcher<TContext, TValue>
{
    public StringMatcher(string needle, Func<TContext, StringSegment, TValue> transform)
        : this(needle, StringComparison.Ordinal, transform) { }

    public ParsedMatch<TValue>? TryMatch(TContext context, StringSegment segment)
    {
        var index = segment.Source.IndexOf(needle, segment.StartIndex, segment.Length, comparison);

        if (index < 0)
            return null;

        var segmentMatch = segment.Relocate(index, needle.Length);
        var value = transform(context, segmentMatch);

        return value is not null ? new ParsedMatch<TValue>(segmentMatch, value) : null;
    }
}
