using System;

namespace DiscordChatExporter.Core.Markdown.Matching
{
    internal class StringMatcher<T> : IMatcher<T>
    {
        private readonly string _needle;
        private readonly StringComparison _comparison;
        private readonly Func<StringPart, T?> _transform;

        public StringMatcher(string needle, StringComparison comparison, Func<StringPart, T?> transform)
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
            if (index < 0)
                return null;

            var stringPartMatch = stringPart.Slice(index, _needle.Length);
            var value = _transform(stringPartMatch);

            return value is not null
                ? new ParsedMatch<T>(stringPartMatch, value)
                : null;
        }
    }
}