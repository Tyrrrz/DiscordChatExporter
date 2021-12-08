using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal interface IMatcher<T>
{
    ParsedMatch<T>? TryMatch(StringPart stringPart);
}

internal static class MatcherExtensions
{
    public static IEnumerable<ParsedMatch<T>> MatchAll<T>(this IMatcher<T> matcher,
        StringPart stringPart, Func<StringPart, T> transformFallback)
    {
        // Loop through segments divided by individual matches
        var currentIndex = stringPart.StartIndex;
        while (currentIndex < stringPart.EndIndex)
        {
            // Find a match within this segment
            var match = matcher.TryMatch(stringPart.Slice(currentIndex, stringPart.EndIndex - currentIndex));

            // If there's no match - break
            if (match is null)
                break;

            // If this match doesn't start immediately at current index - transform and yield fallback first
            if (match.StringPart.StartIndex > currentIndex)
            {
                var fallbackPart = stringPart.Slice(currentIndex, match.StringPart.StartIndex - currentIndex);
                yield return new ParsedMatch<T>(fallbackPart, transformFallback(fallbackPart));
            }

            // Yield match
            yield return match;

            // Shift current index to the end of the match
            currentIndex = match.StringPart.StartIndex + match.StringPart.Length;
        }

        // If EOL wasn't reached - transform and yield remaining part as fallback
        if (currentIndex < stringPart.EndIndex)
        {
            var fallbackPart = stringPart.Slice(currentIndex);
            yield return new ParsedMatch<T>(fallbackPart, transformFallback(fallbackPart));
        }
    }
}