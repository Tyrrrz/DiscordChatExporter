using System;

namespace DiscordChatExporter.Domain.Markdown.Matching
{
    internal class StringMatcher<T> : IMatcher<T>
    {
        private readonly string _needle;
        private readonly StringComparison _comparison;
        private readonly Func<StringPart, T> _transform;

        public StringMatcher(string needle, StringComparison comparison, Func<StringPart, T> transform)
        {
            _needle = needle;
            _comparison = comparison;
            _transform = transform;
        }

        public StringMatcher(string needle, Func<StringPart, T> transform)
            : this(needle, StringComparison.Ordinal, transform)
        {
        }

        public ParsedMatch<T>? TryMatch(StringPart stringPart)
        {
            var index = stringPart.Target.IndexOf(_needle, stringPart.StartIndex, stringPart.Length, _comparison);

            if (index >= 0)
            {
                var stringPartMatch = stringPart.Slice(index, _needle.Length);
                return new ParsedMatch<T>(stringPartMatch, _transform(stringPartMatch));
            }

            return null;
        }
    }
}