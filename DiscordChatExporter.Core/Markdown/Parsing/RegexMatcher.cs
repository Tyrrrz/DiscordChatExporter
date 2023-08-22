using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class RegexMatcher<T> : IMatcher<T>
{
    private readonly Regex _regex;
    private readonly Func<StringSegment, Match, T?> _transform;

    public RegexMatcher(Regex regex, Func<StringSegment, Match, T?> transform)
    {
        _regex = regex;
        _transform = transform;
    }

    public ParsedMatch<T>? TryMatch(StringSegment segment)
    {
        var match = _regex.Match(segment.Source, segment.StartIndex, segment.Length);
        if (!match.Success)
            return null;

        // Overload regex.Match(string, int, int) doesn't take the whole string into account,
        // it effectively functions as a match check on a substring.
        // Which is super weird because regex.Match(string, int) takes the whole input in context.
        // So in order to properly account for ^/$ regex tokens, we need to make sure that
        // the expression also matches on the bigger part of the input.
        if (!_regex.IsMatch(segment.Source[..segment.EndIndex], segment.StartIndex))
            return null;

        var segmentMatch = segment.Relocate(match);
        var value = _transform(segmentMatch, match);

        return value is not null ? new ParsedMatch<T>(segmentMatch, value) : null;
    }
}
