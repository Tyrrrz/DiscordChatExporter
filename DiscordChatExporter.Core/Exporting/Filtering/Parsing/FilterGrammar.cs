using System.Linq;
using DiscordChatExporter.Core.Utils.Extensions;
using Superpower;
using Superpower.Parsers;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing
{
    internal static class FilterGrammar
    {
        // Choice(a, b) looks cleaner than a.Or(b)
        private static TextParser<T> Choice<T>(params TextParser<T>[] parsers) =>
            parsers.Aggregate((current, next) => current.Or(next));

        private static readonly TextParser<char> EscapedCharacter =
            Character.EqualTo('\\').IgnoreThen(Character.AnyChar);

        private static readonly TextParser<string> QuotedString =
            from open in Character.In('"', '\'')
            from value in Choice(EscapedCharacter, Character.Except(open)).Many().Text()
            from close in Character.EqualTo(open)
            select value;

        private static readonly TextParser<char> FreeCharacter =
            Character.Matching(c =>
                    !char.IsWhiteSpace(c) &&
                    // Avoid all special tokens used by the grammar
                    c is not ('(' or ')' or '"' or '\'' or '-' or '|' or '&'),
                "any character except whitespace or `(`, `)`, `\"`, `'`, `-`, `|`, `&`"
            );

        private static readonly TextParser<string> UnquotedString =
            Choice(EscapedCharacter, FreeCharacter).AtLeastOnce().Text();

        private static readonly TextParser<string> String =
            Choice(QuotedString, UnquotedString).Named("text string");

        private static readonly TextParser<MessageFilter> ContainsFilter =
            String.Select(v => (MessageFilter) new ContainsMessageFilter(v));

        private static readonly TextParser<MessageFilter> FromFilter = Span
            .EqualToIgnoreCase("from:")
            .IgnoreThen(String)
            .Select(v => (MessageFilter) new FromMessageFilter(v))
            .Named("from:<value>");

        private static readonly TextParser<MessageFilter> MentionsFilter = Span
            .EqualToIgnoreCase("mentions:")
            .IgnoreThen(String)
            .Select(v => (MessageFilter) new MentionsMessageFilter(v))
            .Named("mentions:<value>");

        private static readonly TextParser<MessageFilter> HasFilter = Span
            .EqualToIgnoreCase("has:")
            .IgnoreThen(Choice(
                Span.EqualToIgnoreCase("link").IgnoreThen(Parse.Return(MessageContentMatchKind.Link)),
                Span.EqualToIgnoreCase("embed").IgnoreThen(Parse.Return(MessageContentMatchKind.Embed)),
                Span.EqualToIgnoreCase("file").IgnoreThen(Parse.Return(MessageContentMatchKind.File)),
                Span.EqualToIgnoreCase("video").IgnoreThen(Parse.Return(MessageContentMatchKind.Video)),
                Span.EqualToIgnoreCase("image").IgnoreThen(Parse.Return(MessageContentMatchKind.Image)),
                Span.EqualToIgnoreCase("sound").IgnoreThen(Parse.Return(MessageContentMatchKind.Sound))
            ))
            .Select(k => (MessageFilter) new HasMessageFilter(k))
            .Named("has:<value>");

        private static readonly TextParser<MessageFilter> NegatedFilter = Character
            .EqualTo('-')
            .IgnoreThen(Parse.Ref(() => StandaloneFilter))
            .Select(f => (MessageFilter) new NegatedMessageFilter(f));

        private static readonly TextParser<MessageFilter> GroupedFilter =
            from open in Character.EqualTo('(')
            from content in Parse.Ref(() => BinaryExpressionFilter).Token()
            from close in Character.EqualTo(')')
            select content;

        private static readonly TextParser<MessageFilter> StandaloneFilter = Choice(
            GroupedFilter,
            FromFilter,
            MentionsFilter,
            HasFilter,
            ContainsFilter
        );

        private static readonly TextParser<MessageFilter> UnaryExpressionFilter = Choice(
            NegatedFilter,
            StandaloneFilter
        );

        private static readonly TextParser<MessageFilter> BinaryExpressionFilter = Parse.Chain(
            Choice(
                // Explicit operator
                Character.In('|', '&').Token().Try(),
                // Implicit operator (resolves to 'and')
                Character.WhiteSpace.AtLeastOnce().IgnoreThen(Parse.Return(' '))
            ),
            UnaryExpressionFilter,
            (op, left, right) => op switch
            {
                '|' => new BinaryExpressionMessageFilter(left, right, BinaryExpressionKind.Or),
                _ => new BinaryExpressionMessageFilter(left, right, BinaryExpressionKind.And)
            }
        );

        public static readonly TextParser<MessageFilter> Filter =
            BinaryExpressionFilter.Token().AtEnd();
    }
}