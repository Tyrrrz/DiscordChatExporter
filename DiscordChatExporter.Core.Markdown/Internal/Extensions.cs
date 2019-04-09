using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal static class Extensions
    {
        public static IEnumerable<ParsedMatch<T>> MatchAll<T>(this IMatcher<T> matcher, string input,
            int startIndex, int length, Func<string, T> fallbackTransform)
        {
            // Get end index for simplicity
            var endIndex = startIndex + length;

            // Loop through segments divided by individual matches
            var currentIndex = startIndex;
            while (currentIndex < endIndex)
            {
                // Find a match within this segment
                var match = matcher.Match(input, currentIndex, endIndex - currentIndex);

                // If there's no match - break
                if (match == null)
                    break;

                // If this match doesn't start immediately at current index - transform and yield fallback first
                if (match.StartIndex > currentIndex)
                {
                    var fallback = input.Substring(currentIndex, match.StartIndex - currentIndex);
                    yield return new ParsedMatch<T>(currentIndex, fallback.Length, fallbackTransform(fallback));
                }

                // Yield match
                yield return match;

                // Shift current index to the end of the match
                currentIndex = match.StartIndex + match.Length;
            }

            // If EOL wasn't reached - transform and yield remaining part as fallback
            if (currentIndex < endIndex)
            {
                var fallback = input.Substring(currentIndex);
                yield return new ParsedMatch<T>(currentIndex, fallback.Length, fallbackTransform(fallback));
            }
        }

        public static IEnumerable<ParsedMatch<T>> MatchAll<T>(this IMatcher<T> matcher, string input,
            Func<string, T> fallbackTransform) => matcher.MatchAll(input, 0, input.Length, fallbackTransform);
    }
}