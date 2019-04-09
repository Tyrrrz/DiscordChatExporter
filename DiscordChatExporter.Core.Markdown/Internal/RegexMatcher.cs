using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class RegexMatcher<T> : IMatcher<T>
    {
        private readonly Regex _regex;
        private readonly Func<Match, T> _transform;

        public RegexMatcher(Regex regex, Func<Match, T> transform)
        {
            _regex = regex;
            _transform = transform;
        }

        public ParsedMatch<T> Match(string input, int startIndex, int length)
        {
            var match = _regex.Match(input, startIndex, length);
            return match.Success ? new ParsedMatch<T>(match.Index, match.Length, _transform(match)) : null;
        }
    }
}