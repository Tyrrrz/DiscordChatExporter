using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal static class Extensions
    {
        public static StringPart Shrink(this StringPart stringPart, int newStartIndex, int newLength) =>
            new StringPart(stringPart.Target, newStartIndex, newLength);

        public static StringPart Shrink(this StringPart stringPart, int newStartIndex) =>
            stringPart.Shrink(newStartIndex, stringPart.EndIndex - newStartIndex);

        public static StringPart Shrink(this StringPart stringPart, Capture capture) =>
            stringPart.Shrink(capture.Index, capture.Length);

        public static IEnumerable<ParsedMatch<T>> MatchAll<T>(this IMatcher<T> matcher, StringPart stringPart,
            Func<StringPart, T> fallbackTransform)
        {
            // Loop through segments divided by individual matches
            var currentIndex = stringPart.StartIndex;
            while (currentIndex < stringPart.EndIndex)
            {
                // Find a match within this segment
                var match = matcher.Match(stringPart.Shrink(currentIndex, stringPart.EndIndex - currentIndex));

                // If there's no match - break
                if (match == null)
                    break;

                // If this match doesn't start immediately at current index - transform and yield fallback first
                if (match.StringPart.StartIndex > currentIndex)
                {
                    var fallbackPart = stringPart.Shrink(currentIndex, match.StringPart.StartIndex - currentIndex);
                    yield return new ParsedMatch<T>(fallbackPart, fallbackTransform(fallbackPart));
                }

                // Yield match
                yield return match;

                // Shift current index to the end of the match
                currentIndex = match.StringPart.StartIndex + match.StringPart.Length;
            }

            // If EOL wasn't reached - transform and yield remaining part as fallback
            if (currentIndex < stringPart.EndIndex)
            {
                var fallbackPart = stringPart.Shrink(currentIndex);
                yield return new ParsedMatch<T>(fallbackPart, fallbackTransform(fallbackPart));
            }
        }
    }
}