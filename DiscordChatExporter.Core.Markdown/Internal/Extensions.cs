using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal static class Extensions
    {
        public static void Sort<T, TKey>(this List<T> list, Func<T, TKey> keySelector) =>
            list.Sort((x, y) => Comparer<TKey>.Default.Compare(keySelector(x), keySelector(y)));

        public static IReadOnlyList<ParseResult<T>> Parse<T>(this IParser<T> parser, string input) =>
            parser.Parse(input, 0, input.Length);
    }
}