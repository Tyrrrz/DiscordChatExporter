using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class RegexParser<T> : IParser<T>
    {
        private readonly Regex _regex;
        private readonly Func<Match, T> _transform;

        public RegexParser(Regex regex, Func<Match, T> transform)
        {
            _regex = regex;
            _transform = transform;
        }

        public IReadOnlyList<ParseResult<T>> Parse(string input, int startIndex, int length)
        {
            var results = new List<ParseResult<T>>();

            // Find first match on the initial range
            var match = _regex.Match(input, startIndex, length);

            // Loop while matches are successful
            while (match.Success)
            {
                // Transform and add result from last match
                var result = new ParseResult<T>(_transform(match), match.Index, match.Length);
                results.Add(result);

                // Find next match on a subrange of the initial range
                match = _regex.Match(input, match.Index + match.Length, startIndex + length - match.Index - match.Length);
            }

            return results;
        }
    }
}