using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal static class Extensions
    {
        public static IEnumerable<Match> Matches(this Regex regex, string input, int startIndex, int length)
        {
            // Find first match on the initial range
            var match = regex.Match(input, startIndex, length);

            // Loop while matches are successful
            while (match.Success)
            {
                // Yield last match
                yield return match;

                // Find next match on a subrange of the initial range
                match = regex.Match(input, match.Index + match.Length, startIndex + length - match.Index - match.Length);
            }
        }

        public static void Sort<T, TKey>(this List<T> list, Func<T, TKey> keySelector) =>
            list.Sort((x, y) => Comparer<TKey>.Default.Compare(keySelector(x), keySelector(y)));
    }
}