using System;

namespace DiscordChatExporter.Core.Markdown.Internal
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

        public ParsedMatch<T> Match(StringPart stringPart)
        {
            var index = stringPart.Target.IndexOf(_needle, stringPart.StartIndex, stringPart.Length, _comparison);

            if (index >= 0)
            {
                var stringPartShrunk = stringPart.Shrink(index, _needle.Length);
                return new ParsedMatch<T>(stringPartShrunk, _transform(stringPartShrunk));
            }

            return null;
        }
    }
}