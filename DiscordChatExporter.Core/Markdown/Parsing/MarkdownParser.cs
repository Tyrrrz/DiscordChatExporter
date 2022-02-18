using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Markdown.Parsing;

// Discord does NOT use a recursive-descent parser for markdown which becomes evident in some
// scenarios, like when multiple formatting nodes are nested together.
// To replicate Discord's behavior, we're employing a special parser that uses a set of regular
// expressions that are executed sequentially in a first-match-first-serve manner.
internal static partial class MarkdownParser
{
    private const RegexOptions DefaultRegexOptions =
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant |
        RegexOptions.Multiline;

    /* Formatting */

    // Capture any character until the earliest double asterisk not followed by an asterisk
    private static readonly IMatcher<MarkdownNode> BoldFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Bold, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any character until the earliest single asterisk not preceded or followed by an asterisk
    // Opening asterisk must not be followed by whitespace
    // Closing asterisk must not be preceded by whitespace
    private static readonly IMatcher<MarkdownNode> ItalicFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any character until the earliest triple asterisk not followed by an asterisk
    private static readonly IMatcher<MarkdownNode> ItalicBoldFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\*(\\*\\*.+?\\*\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1]), BoldFormattingNodeMatcher))
    );

    // Capture any character except underscore until an underscore
    // Closing underscore must not be followed by a word character
    private static readonly IMatcher<MarkdownNode> ItalicAltFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("_([^_]+)_(?!\\w)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Italic, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any character until the earliest double underscore not followed by an underscore
    private static readonly IMatcher<MarkdownNode> UnderlineFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Underline, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any character until the earliest triple underscore not followed by an underscore
    private static readonly IMatcher<MarkdownNode> ItalicUnderlineFormattingNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            new Regex("_(__.+?__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
            (s, m) => new FormattingNode(FormattingKind.Italic,
                Parse(s.Relocate(m.Groups[1]), UnderlineFormattingNodeMatcher))
        );

    // Capture any character until the earliest double tilde
    private static readonly IMatcher<MarkdownNode> StrikethroughFormattingNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            new Regex("~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
            (s, m) => new FormattingNode(FormattingKind.Strikethrough, Parse(s.Relocate(m.Groups[1])))
        );

    // Capture any character until the earliest double pipe
    private static readonly IMatcher<MarkdownNode> SpoilerFormattingNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\|\\|(.+?)\\|\\|", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Spoiler, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any character until the end of the line
    // Opening 'greater than' character must be followed by whitespace
    private static readonly IMatcher<MarkdownNode> SingleLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("^>\\s(.+\n?)", DefaultRegexOptions),
        (s, m) => new FormattingNode(FormattingKind.Quote, Parse(s.Relocate(m.Groups[1])))
    );

    // Repeatedly capture any character until the end of the line
    // This one is tricky as it ends up producing multiple separate captures which need to be joined
    private static readonly IMatcher<MarkdownNode> RepeatedSingleLineQuoteNodeMatcher =
        new RegexMatcher<MarkdownNode>(
            new Regex("(?:^>\\s(.+\n?)){2,}", DefaultRegexOptions),
            (_, m) =>
            {
                var content = string.Concat(m.Groups[1].Captures.Select(c => c.Value));
                return new FormattingNode(FormattingKind.Quote, Parse(content));
            }
        );

    // Capture any character until the end of the input
    // Opening 'greater than' characters must be followed by whitespace
    private static readonly IMatcher<MarkdownNode> MultiLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("^>>>\\s(.+)", DefaultRegexOptions | RegexOptions.Singleline),
        (s, m) => new FormattingNode(FormattingKind.Quote, Parse(s.Relocate(m.Groups[1])))
    );

    /* Code blocks */

    // Capture any character except backtick until a backtick
    // Blank lines at the beginning and end of content are trimmed
    // There can be either one or two backticks, but equal number on both sides
    private static readonly IMatcher<MarkdownNode> InlineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("(`{1,2})([^`]+)\\1", DefaultRegexOptions | RegexOptions.Singleline),
        (_, m) => new InlineCodeBlockNode(m.Groups[2].Value.Trim('\r', '\n'))
    );

    // Capture language identifier and then any character until the earliest triple backtick
    // Language identifier is one word immediately after opening backticks, followed immediately by newline
    // Blank lines at the beginning and end of content are trimmed
    private static readonly IMatcher<MarkdownNode> MultiLineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("```(?:(\\w*)\\n)?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
        (_, m) => new MultiLineCodeBlockNode(m.Groups[1].Value, m.Groups[2].Value.Trim('\r', '\n'))
    );

    /* Mentions */

    // Capture @everyone
    private static readonly IMatcher<MarkdownNode> EveryoneMentionNodeMatcher = new StringMatcher<MarkdownNode>(
        "@everyone",
        _ => new MentionNode(null, MentionKind.Everyone)
    );

    // Capture @here
    private static readonly IMatcher<MarkdownNode> HereMentionNodeMatcher = new StringMatcher<MarkdownNode>(
        "@here",
        _ => new MentionNode(null, MentionKind.Here)
    );

    // Capture <@123456> or <@!123456>
    private static readonly IMatcher<MarkdownNode> UserMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<@!?(\\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.User)
    );

    // Capture <#123456>
    private static readonly IMatcher<MarkdownNode> ChannelMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<#!?(\\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.Channel)
    );

    // Capture <@&123456>
    private static readonly IMatcher<MarkdownNode> RoleMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<@&(\\d+)>", DefaultRegexOptions),
        (_, m) => new MentionNode(Snowflake.TryParse(m.Groups[1].Value), MentionKind.Role)
    );

    /* Emoji */

    // Capture any country flag emoji (two regional indicator surrogate pairs)
    // ... or "miscellaneous symbol" character
    // ... or surrogate pair
    // ... or digit followed by enclosing mark
    // (this does not match all emoji in Discord but it's reasonably accurate enough)
    private static readonly IMatcher<MarkdownNode> StandardEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("((?:[\\uD83C][\\uDDE6-\\uDDFF]){2}|[\\u2600-\\u26FF]|\\p{Cs}{2}|\\d\\p{Me})", DefaultRegexOptions),
        (_, m) => new EmojiNode(m.Groups[1].Value)
    );

    // Capture :thinking: (but only for known emoji codes)
    private static readonly IMatcher<MarkdownNode> CodedStandardEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex(":([\\w_]+):", DefaultRegexOptions),
        (_, m) =>
        {
            var name = EmojiIndex.TryGetName(m.Groups[1].Value);
            return !string.IsNullOrWhiteSpace(name)
                ? new EmojiNode(name)
                : null;
        }
    );

    // Capture <:lul:123456> or <a:lul:123456>
    private static readonly IMatcher<MarkdownNode> CustomEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<(a)?:(.+?):(\\d+?)>", DefaultRegexOptions),
        (_, m) => new EmojiNode(
            Snowflake.TryParse(m.Groups[3].Value),
            m.Groups[2].Value,
            !string.IsNullOrWhiteSpace(m.Groups[1].Value)
        )
    );

    /* Links */

    // Capture [title](link)
    private static readonly IMatcher<MarkdownNode> TitledLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\[(.+?)\\]\\((.+?)\\)", DefaultRegexOptions),
        (s, m) => new LinkNode(m.Groups[2].Value, Parse(s.Relocate(m.Groups[1])))
    );

    // Capture any non-whitespace character after http:// or https://
    // until the last punctuation character or whitespace
    private static readonly IMatcher<MarkdownNode> AutoLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("(https?://\\S*[^\\.,:;\"\'\\s])", DefaultRegexOptions),
        (_, m) => new LinkNode(m.Groups[1].Value)
    );

    // Same as auto link but also surrounded by angular brackets
    private static readonly IMatcher<MarkdownNode> HiddenLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<(https?://\\S*[^\\.,:;\"\'\\s])>", DefaultRegexOptions),
        (_, m) => new LinkNode(m.Groups[1].Value)
    );

    /* Text */

    // Capture the shrug kaomoji
    // This escapes it from matching for formatting
    private static readonly IMatcher<MarkdownNode> ShrugTextNodeMatcher = new StringMatcher<MarkdownNode>(
        @"¯\_(ツ)_/¯",
        s => new TextNode(s.ToString())
    );

    // Capture some specific emoji that don't get rendered
    // This escapes it from matching for emoji
    private static readonly IMatcher<MarkdownNode> IgnoredEmojiTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("(\\u26A7|\\u2640|\\u2642|\\u2695|\\u267E|\\u00A9|\\u00AE|\\u2122)", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    // Capture any "symbol/other" character or surrogate pair preceded by a backslash
    // This escapes it from matching for emoji
    private static readonly IMatcher<MarkdownNode> EscapedSymbolTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\\\(\\p{So}|\\p{Cs}{2})", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    // Capture any non-whitespace, non latin alphanumeric character preceded by a backslash
    // This escapes it from matching for formatting or other tokens
    private static readonly IMatcher<MarkdownNode> EscapedCharacterTextNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("\\\\([^a-zA-Z0-9\\s])", DefaultRegexOptions),
        (_, m) => new TextNode(m.Groups[1].Value)
    );

    /* Misc */

    // Capture <t:12345678> or <t:12345678:R>
    private static readonly IMatcher<MarkdownNode> UnixTimestampNodeMatcher = new RegexMatcher<MarkdownNode>(
        new Regex("<t:(-?\\d+)(?::\\w)?>", DefaultRegexOptions),
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