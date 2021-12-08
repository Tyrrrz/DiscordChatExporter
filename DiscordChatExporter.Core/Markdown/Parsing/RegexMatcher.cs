using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class RegexMatcher<T> : IMatcher<T>
{
    private readonly Regex _regex;
    private readonly Func<StringPart, Match, T?> _transform;

    public RegexMatcher(Regex regex, Func<StringPart, Match, T?> transform)
    {
        _regex = regex;
        _transform = transform;
    }

    public ParsedMatch<T>? TryMatch(StringPart stringPart)
    {
        var match = _regex.Match(stringPart.Target, stringPart.StartIndex, stringPart.Length);
        if (!match.Success)
            return null;

        // Overload regex.Match(string, int, int) doesn't take the whole string into account,
        // it effectively functions as a match check on a substring.
        // Which is super weird because regex.Match(string, int) takes the whole input in context.
        // So in order to properly account for ^/$ regex tokens, we need to make sure that
        // the expression also matches on the bigger part of the input.
        if (!_regex.IsMatch(stringPart.Target[..stringPart.EndIndex], stringPart.StartIndex))
            return null;

        var stringPartMatch = stringPart.Slice(match.Index, match.Length);
        var value = _transform(stringPartMatch, match);

        return value is not null
            ? new ParsedMatch<T>(stringPartMatch, value)
            : null;
    }
}