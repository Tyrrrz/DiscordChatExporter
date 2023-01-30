using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Markdown.Parsing;

// Discord does NOT use a recursive-descent parser for markdown which becomes evident in some
// scenarios, like when multiple formatting nodes are nested together.
// To replicate Discord's behavior, we're employing a special parser that uses a set of regular
// expressions that are executed sequentially in a first-match-first-serve manner.
internal static partial class MarkdownParser
{
    private const RegexOptions DefaultRegexOptions =
        RegexOptions.Compiled |
        RegexOptions.IgnorePatternWhitespace |
        RegexOptions.CultureInvariant |
        RegexOptions.Multiline;

    /* Formatting */

    private static readonly IMatcher<MarkdownNode> BoldFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the earliest double asterisk not followed by an asterisk
        new Regex(@"\*\*(.+?)\*\*(?!\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Bold, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> ItalicFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the earliest single asterisk not preceded or followed by an asterisk
        // Opening asterisk must not be followed by whitespace
        // Closing asterisk must not be preceded by whitespace
        new Regex(@"\*(?!\s)(.+?)(?<!\s|\*)\*(?!\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> ItalicBoldFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the earliest triple asterisk not followed by an asterisk
        new Regex(@"\*(\*\*.+?\*\*)\*(?!\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1]), BoldFormattingNodeMatcher))
    );

    private static readonly IMatcher<MarkdownNode> ItalicAltFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character except underscore until an underscore
        // Closing underscore must not be followed by a word character
        new Regex(@"_([^_]+)_(?!\w)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> UnderlineFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the earliest double underscore not followed by an underscore
        new Regex(@"__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Underline, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> ItalicUnderlineFormattingNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            // Capture any character until the earliest triple underscore not followed by an underscore
            new Regex(@"_(__.+?__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
            (s, m) => new FormattingNode(
                FormattingKind.Italic,
                Parse(s.Relocate(m.Groups[1]), UnderlineFormattingNodeMatcher)
            )
        );

    private static readonly IMatcher<MarkdownNode> StrikethroughFormattingNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            // Capture any character until the earliest double tilde
            new Regex(@"~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
            (s, m) => new FormattingNode(FormattingKind.Strikethrough, Parse(s.Relocate(m.Groups[1])))
        );

    private static readonly IMatcher<MarkdownNode> SpoilerFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the earliest double pipe
        new Regex(@"\|\|(.+?)\|\|", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Spoiler, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> SingleLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the end of the line
        // Opening 'greater than' character must be followed by whitespace
        // Text content is optional
        new Regex(@"^>\s(.*\n?)", DefaultRegexOptions),
        (s, m) => new FormattingNode(FormattingKind.Quote, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> RepeatedSingleLineQuoteNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            // Repeatedly capture any character until the end of the line
            // This one is tricky as it ends up producing multiple separate captures which need to be joined
            new Regex(@"(?:^>\s(.*\n?)){2,}", DefaultRegexOptions),
            (_, m) => new FormattingNode(
                FormattingKind.Quote,
                Parse(
                    // Combine all captures into a single string
                    string.Concat(m.Groups[1].Captures.Select(c => c.Value))
                )
            )
        );

    private static readonly IMatcher<MarkdownNode> MultiLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character until the end of the input
        // Opening 'greater than' characters must be followed by whitespace
        new Regex(@"^>>>\s(.+)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Quote, Parse(s.Relocate(m.Groups[1])))
    );

    /* Code blocks */

    private static readonly IMatcher<MarkdownNode> InlineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any character except backtick until a backtick
        // Blank lines at the beginning and end of content are trimmed
        // There can be either one or two backticks, but equal number on both sides
        new Regex(@"(`{1,2})([^`]+)\1", DefaultRegexOptions | RegexOptions.Singleline),
        (_, m) => new InlineCodeBlockNode(m.Groups[2].Value.Trim('\r', '\n'))
    );

    private static readonly IMatcher<MarkdownNode> MultiLineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture language identifier and then any character until the earliest triple backtick
        // Language identifier is one word immediately after opening backticks, followed immediately by newline
        // Blank lines at the beginning and end of content are trimmed
        new Regex(@"```(?:(\w*)\n)?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
        (_, m) => new MultiLineCodeBlockNode(m.Groups[1].Value, m.Groups[2].Value.Trim('\r', '\n'))
    );

    /* Mentions */

    private static readonly IMatcher<MarkdownNode> EveryoneMentionNodeMatcher = new StringMatcher<MarkdownNode>(
        "@everyone",
        _ => new MentionNode(null, MentionKind.Everyone)
    );

    private static readonly IMatcher<MarkdownNode> HereMentionNodeMatcher = new StringMatcher<MarkdownNode>(
        "@here",
        _ => new MentionNode(null, MentionKind.Here)
    );

    private static readonly IMatcher<MarkdownNode> UserMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture <@123456> or <@!123456>
        new Regex(@"<@!?(\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.User)
    );

    private static readonly IMatcher<MarkdownNode> ChannelMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture <#123456>
        new Regex(@"<\#!?(\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.Channel)
    );

    private static readonly IMatcher<MarkdownNode> RoleMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture <@&123456>
        new Regex(@"<@&(\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.Role)
    );

    /* Emoji */

    private static readonly IMatcher<MarkdownNode> StandardEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex(
            @"(" +
            // Country flag emoji (two regional indicator surrogate pairs)
            @"(?:\uD83C[\uDDE6-\uDDFF]){2}|" +
            // Digit emoji (digit followed by enclosing mark)
            @"\d\p{Me}|" +
            // Surrogate pair
            @"\p{Cs}{2}|" +
            // Miscellaneous characters
            @"[" +
            @"\u2600-\u2604" +
            @"\u260E\u2611" +
            @"\u2614-\u2615" +
            @"\u2618\u261D\u2620" +
            @"\u2622-\u2623" +
            @"\u2626\u262A" +
            @"\u262E-\u262F" +
            @"\u2638-\u263A" +
            @"\u2640\u2642" +
            @"\u2648-\u2653" +
            @"\u265F-\u2660" +
            @"\u2663" +
            @"\u2665-\u2666" +
            @"\u2668\u267B" +
            @"\u267E-\u267F" +
            @"\u2692-\u2697" +
            @"\u2699" +
            @"\u269B-\u269C" +
            @"\u26A0-\u26A1" +
            @"\u26A7" +
            @"\u26AA-\u26AB" +
            @"\u26B0-\u26B1" +
            @"\u26BD-\u26BE" +
            @"\u26C4-\u26C5" +
            @"\u26C8" +
            @"\u26CE-\u26CF" +
            @"\u26D1" +
            @"\u26D3-\u26D4" +
            @"\u26E9-\u26EA" +
            @"\u26F0-\u26F5" +
            @"\u26F7-\u26FA" +
            @"\u26FD" +
            @"]" +
            @")", DefaultRegexOptions),
        (_, m) => new EmojiNode(m.Groups[1].Value)
    );

    private static readonly IMatcher<MarkdownNode> CodedStandardEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture :thinking: for known emoji codes
        new Regex(@":([\w_]+):", DefaultRegexOptions),
        (_, m) => EmojiIndex.TryGetName(m.Groups[1].Value)?.Pipe(n => new EmojiNode(n))
    );

    private static readonly IMatcher<MarkdownNode> CustomEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture <:lul:123456> or <a:lul:123456>
        new Regex(@"<(a)?:(.+?):(\d+?)>", DefaultRegexOptions),
        (_, m) => new EmojiNode(
            Snowflake.TryParse(m.Groups[3].Value),
            m.Groups[2].Value,
            !string.IsNullOrWhiteSpace(m.Groups[1].Value)
        )
    );

    /* Links */

    private static readonly IMatcher<MarkdownNode> TitledLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture [title](link)
        new Regex(@"\[(.+?)\]\((.+?)\)", DefaultRegexOptions),
        (s, m) => new LinkNode(m.Groups[2].Value, Parse(s.Relocate(m.Groups[1])))
    );

    private static readonly IMatcher<MarkdownNode> AutoLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any non-whitespace character after http:// or https://
        // until the last punctuation character or whitespace
        new Regex(@"(https?://\S*[^\.,:;""'\s])", DefaultRegexOptions),
        (_, m) => new LinkNode(m.Groups[1].Value)
    );

    private static readonly IMatcher<MarkdownNode> HiddenLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Same as auto link but also surrounded by angular brackets
        new Regex(@"<(https?://\S*[^\.,:;""'\s])>", DefaultRegexOptions),
        (_, m) => new LinkNode(m.Groups[1].Value)
    );

    /* Text */

    private static readonly IMatcher<MarkdownNode> ShrugTextNodeMatcher = new StringMatcher<MarkdownNode>(
        // Capture the shrug kaomoji
        // This escapes it from matching for formatting
        @"¯\_(ツ)_/¯",
        s => new TextNode(s.ToString())
    );

    private static readonly IMatcher<MarkdownNode> IgnoredEmojiTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture some specific emoji that don't get rendered
        // This escapes it from matching for emoji
        new Regex(@"([\u26A7\u2640\u2642\u2695\u267E\u00A9\u00AE\u2122])", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    private static readonly IMatcher<MarkdownNode> EscapedSymbolTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any "symbol/other" character or surrogate pair preceded by a backslash
        // This escapes it from matching for emoji
        new Regex(@"\\(\p{So}|\p{Cs}{2})", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    private static readonly IMatcher<MarkdownNode> EscapedCharacterTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture any non-whitespace, non latin alphanumeric character preceded by a backslash
        // This escapes it from matching for formatting or other tokens
        new Regex(@"\\([^a-zA-Z0-9\s])", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    /* Misc */

    private static readonly IMatcher<MarkdownNode> UnixTimestampNodeMatcher = new RegexMatcher<MarkdownNode>(
        // Capture <t:12345678> or <t:12345678:R>
        new Regex(@"<t:(-?\d+)(?::\w)?>", DefaultRegexOptions),
        (_, m) =>
        {
            // TODO: support formatting parameters
            // See: https://github.com/Tyrrrz/DiscordChatExporter/issues/662

            if (!long.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var offset))
            {
                return new UnixTimestampNode(null);
            }

            try
            {
                return new UnixTimestampNode(DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(offset));
            }
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/681
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/766
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or OverflowException)
            {
                return new UnixTimestampNode(null);
            }
        }
    );

    // Combine all matchers into one
    // Matchers that have similar patterns are ordered from most specific to least specific
    private static readonly IMatcher<MarkdownNode> AggregateNodeMatcher = new AggregateMatcher<MarkdownNode>(
        // Escaped text
        ShrugTextNodeMatcher,
        IgnoredEmojiTextNodeMatcher,
        EscapedSymbolTextNodeMatcher,
        EscapedCharacterTextNodeMatcher,

        // Formatting
        ItalicBoldFormattingNodeMatcher,
        ItalicUnderlineFormattingNodeMatcher,
        BoldFormattingNodeMatcher,
        ItalicFormattingNodeMatcher,
        UnderlineFormattingNodeMatcher,
        ItalicAltFormattingNodeMatcher,
        StrikethroughFormattingNodeMatcher,
        SpoilerFormattingNodeMatcher,
        MultiLineQuoteNodeMatcher,
        RepeatedSingleLineQuoteNodeMatcher,
        SingleLineQuoteNodeMatcher,

        // Code blocks
        MultiLineCodeBlockNodeMatcher,
        InlineCodeBlockNodeMatcher,

        // Mentions
        EveryoneMentionNodeMatcher,
        HereMentionNodeMatcher,
        UserMentionNodeMatcher,
        ChannelMentionNodeMatcher,
        RoleMentionNodeMatcher,

        // Links
        TitledLinkNodeMatcher,
        AutoLinkNodeMatcher,
        HiddenLinkNodeMatcher,

        // Emoji
        StandardEmojiNodeMatcher,
        CustomEmojiNodeMatcher,
        CodedStandardEmojiNodeMatcher,

        // Misc
        UnixTimestampNodeMatcher
    );

    // Minimal set of matchers for non-multimedia formats (e.g. plain text)
    private static readonly IMatcher<MarkdownNode> MinimalAggregateNodeMatcher = new AggregateMatcher<MarkdownNode>(
        // Mentions
        EveryoneMentionNodeMatcher,
        HereMentionNodeMatcher,
        UserMentionNodeMatcher,
        ChannelMentionNodeMatcher,
        RoleMentionNodeMatcher,

        // Emoji
        CustomEmojiNodeMatcher,

        // Misc
        UnixTimestampNodeMatcher
    );

    private static IReadOnlyList<MarkdownNode> Parse(StringSegment segment, IMatcher<MarkdownNode> matcher) =>
        matcher
            .MatchAll(segment, s => new TextNode(s.ToString()))
            .Select(r => r.Value)
            .ToArray();
}

internal static partial class MarkdownParser
{
    private static IReadOnlyList<MarkdownNode> Parse(StringSegment segment) =>
        Parse(segment, AggregateNodeMatcher);

    private static IReadOnlyList<MarkdownNode> ParseMinimal(StringSegment segment) =>
        Parse(segment, MinimalAggregateNodeMatcher);

    public static IReadOnlyList<MarkdownNode> Parse(string input) =>
        Parse(new StringSegment(input));

    public static IReadOnlyList<MarkdownNode> ParseMinimal(string input) =>
        ParseMinimal(new StringSegment(input));
}