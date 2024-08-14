using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class RegexMatcher<TContext, TValue>(
    Regex regex,
    Func<TContext, StringSegment, Match, TValue?> transform
) : IMatcher<TContext, TValue>
{
    public ParsedMatch<TValue>? TryMatch(TContext context, StringSegment segment)
    {
        var match = regex.Match(segment.Source, segment.StartIndex, segment.Length);
        if (!match.Success)
            return null;

        // Overload regex.Match(string, int, int) doesn't take the whole string into account,
        // it effectively functions as a match check on a substring.
        // Which is super weird because regex.Match(string, int) takes the whole input in context.
        // So in order to properly account for ^/$ regex tokens, we need to make sure that
        // the expression also matches on the bigger part of the input.
        if (!regex.IsMatch(segment.Source[..segment.EndIndex], segment.StartIndex))
            return null;

        var segmentMatch = segment.Relocate(match);
        var value = transform(context, segmentMatch, match);

        return value is not null ? new ParsedMatch<TValue>(segmentMatch, value) : null;
    }
}
