using DiscordChatExporter.Core.Utils.Extensions;
using Superpower;
using Superpower.Parsers;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing;

internal static class FilterGrammar
{
    private static readonly TextParser<char> EscapedCharacter = Character
        .EqualTo('\\')
        .IgnoreThen(Character.AnyChar);

    private static readonly TextParser<string> QuotedString =
        from open in Character.In('"', '\'')
        from value in Parse.OneOf(EscapedCharacter, Character.Except(open)).Many().Text()
        from close in Character.EqualTo(open)
        select value;

    private static readonly TextParser<string> UnquotedString = Parse
        .OneOf(
            EscapedCharacter,
            // Avoid whitespace as it's treated as an implicit 'and' operator.
            // Also avoid all special tokens used by other parsers.
            Character.ExceptIn(' ', '(', ')', '"', '\'', '-', '~', '|', '&')
        )
        .AtLeastOnce()
        .Text();

    private static readonly TextParser<string> String = Parse
        .OneOf(QuotedString, UnquotedString)
        .Named("text string");

    private static readonly TextParser<MessageFilter> ContainsFilter = String.Select(v =>
        (MessageFilter)new ContainsMessageFilter(v)
    );

    private static readonly TextParser<MessageFilter> FromFilter = Span.EqualToIgnoreCase("from:")
        .Try()
        .IgnoreThen(String)
        .Select(v => (MessageFilter)new FromMessageFilter(v))
        .Named("from:<value>");

    private static readonly TextParser<MessageFilter> MentionsFilter = Span.EqualToIgnoreCase(
            "mentions:"
        )
        .Try()
        .IgnoreThen(String)
        .Select(v => (MessageFilter)new MentionsMessageFilter(v))
        .Named("mentions:<value>");

    private static readonly TextParser<MessageFilter> ReactionFilter = Span.EqualToIgnoreCase(
            "reaction:"
        )
        .Try()
        .IgnoreThen(String)
        .Select(v => (MessageFilter)new ReactionMessageFilter(v))
        .Named("reaction:<value>");

    private static readonly TextParser<MessageFilter> HasFilter = Span.EqualToIgnoreCase("has:")
        .Try()
        .IgnoreThen(
            Parse.OneOf(
                Span.EqualToIgnoreCase("link")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Link))
                    .Try(),
                Span.EqualToIgnoreCase("embed")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Embed))
                    .Try(),
                Span.EqualToIgnoreCase("file")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.File))
                    .Try(),
                Span.EqualToIgnoreCase("video")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Video))
                    .Try(),
                Span.EqualToIgnoreCase("image")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Image))
                    .Try(),
                Span.EqualToIgnoreCase("sound")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Sound)),
                Span.EqualToIgnoreCase("pin")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Pin))
                    .Try(),
                Span.EqualToIgnoreCase("invite")
                    .IgnoreThen(Parse.Return(MessageContentMatchKind.Invite))
                    .Try()
            )
        )
        .Select(k => (MessageFilter)new HasMessageFilter(k))
        .Named("has:<value>");

    // Make sure that property-based filters like 'has:link' don't prevent text like 'hello' from being parsed.
    // https://github.com/Tyrrrz/DiscordChatExporter/issues/909#issuecomment-1227575455
    private static readonly TextParser<MessageFilter> PrimitiveFilter = Parse.OneOf(
        FromFilter,
        MentionsFilter,
        ReactionFilter,
        HasFilter,
        ContainsFilter
    );

    private static readonly TextParser<MessageFilter> GroupedFilter =
        from open in Character.EqualTo('(')
        from content in Parse.Ref(() => ChainedFilter!).Token()
        from close in Character.EqualTo(')')
        select content;

    private static readonly TextParser<MessageFilter> NegatedFilter = Character
        // Dash is annoying to use from CLI due to conflicts with options, so we provide tilde as an alias
        .In('-', '~')
        .IgnoreThen(Parse.OneOf(GroupedFilter, PrimitiveFilter))
        .Select(f => (MessageFilter)new NegatedMessageFilter(f));

    private static readonly TextParser<MessageFilter> ChainedFilter = Parse.Chain(
        // Operator
        Parse.OneOf(
            // Explicit operator
            Character.In('|', '&').Token().Try(),
            // Implicit operator (resolves to 'and')
            Character.EqualTo(' ').AtLeastOnce().IgnoreThen(Parse.Return(' '))
        ),
        // Operand
        Parse.OneOf(NegatedFilter, GroupedFilter, PrimitiveFilter),
        // Reducer
        (op, left, right) =>
            op switch
            {
                '|' => new BinaryExpressionMessageFilter(left, right, BinaryExpressionKind.Or),
                _ => new BinaryExpressionMessageFilter(left, right, BinaryExpressionKind.And)
            }
    );

    public static readonly TextParser<MessageFilter> Filter = ChainedFilter.Token().AtEnd();
}
