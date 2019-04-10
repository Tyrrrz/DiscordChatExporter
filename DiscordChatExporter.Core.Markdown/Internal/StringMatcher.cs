using System;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class StringMatcher<T> : IMatcher<T>
    {
        private readonly string _needle;
        private readonly StringComparison _comparison;
        private readonly Func<string, T> _transform;

        public StringMatcher(string needle, StringComparison comparison, Func<string, T> transform)
        {
            _needle = needle;
            _comparison = comparison;
            _transform = transform;
        }

        public StringMatcher(string needle, Func<string, T> transform)
            : this(needle, StringComparison.Ordinal, transform)
        {
        }

        public ParsedMatch<T> Match(string input, int startIndex, int length)
        {
            var index = input.IndexOf(_needle, startIndex, length, _comparison);
            return index >= 0 ? new ParsedMatch<T>(index, _needle.Length, _transform(_needle)) : null;
        }
    }
}