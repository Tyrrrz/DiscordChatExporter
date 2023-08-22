using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal interface IMatcher<T>
{
    ParsedMatch<T>? TryMatch(StringSegment segment);
}

internal static class MatcherExtensions
{
    public static IEnumerable<ParsedMatch<T>> MatchAll<T>(
        this IMatcher<T> matcher,
        StringSegment segment,
        Func<StringSegment, T> transformFallback
    )
    {
        // Loop through segments divided by individual matches
        var currentIndex = segment.StartIndex;
        while (currentIndex < segment.EndIndex)
        {
            // Find a match within this segment
            var match = matcher.TryMatch(
                segment.Relocate(currentIndex, segment.EndIndex - currentIndex)
            );

            if (match is null)
                break;

            // If this match doesn't start immediately at the current position - transform and yield fallback first
            if (match.Segment.StartIndex > currentIndex)
            {
                var fallbackSegment = segment.Relocate(
                    currentIndex,
                    match.Segment.StartIndex - currentIndex
                );

                yield return new ParsedMatch<T>(
                    fallbackSegment,
                    transformFallback(fallbackSegment)
                );
            }

            yield return match;

            // Shift current index to the end of the match
            currentIndex = match.Segment.StartIndex + match.Segment.Length;
        }

        // If EOL hasn't been reached - transform and yield remaining part as fallback
        if (currentIndex < segment.EndIndex)
        {
            var fallbackSegment = segment.Relocate(currentIndex, segment.EndIndex - currentIndex);

            yield return new ParsedMatch<T>(fallbackSegment, transformFallback(fallbackSegment));
        }
    }
}
