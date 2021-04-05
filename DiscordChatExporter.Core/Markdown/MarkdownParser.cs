﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Markdown.Ast;
using DiscordChatExporter.Core.Markdown.Matching;

namespace DiscordChatExporter.Core.Markdown
{
    // The following parsing logic is meant to replicate Discord's markdown grammar as close as possible
    internal static partial class MarkdownParser
    {
        private const RegexOptions DefaultRegexOptions =
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.Multiline;

        /* Formatting */

        // Capture any character until the earliest double asterisk not followed by an asterisk
        private static readonly IMatcher<MarkdownNode> BoldFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Bold, Parse(p.Slice(m.Groups[1])))
        );

        // Capture any character until the earliest single asterisk not preceded or followed by an asterisk
        // Opening asterisk must not be followed by whitespace
        // Closing asterisk must not be preceded by whitespace
        private static readonly IMatcher<MarkdownNode> ItalicFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Italic, Parse(p.Slice(m.Groups[1])))
        );

        // Capture any character until the earliest triple asterisk not followed by an asterisk
        private static readonly IMatcher<MarkdownNode> ItalicBoldFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\*(\\*\\*.+?\\*\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Italic, Parse(p.Slice(m.Groups[1]), BoldFormattedNodeMatcher))
        );

        // Capture any character except underscore until an underscore
        // Closing underscore must not be followed by a word character
        private static readonly IMatcher<MarkdownNode> ItalicAltFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("_([^_]+)_(?!\\w)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Italic, Parse(p.Slice(m.Groups[1])))
        );

        // Capture any character until the earliest double underscore not followed by an underscore
        private static readonly IMatcher<MarkdownNode> UnderlineFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Underline, Parse(p.Slice(m.Groups[1])))
        );

        // Capture any character until the earliest triple underscore not followed by an underscore
        private static readonly IMatcher<MarkdownNode> ItalicUnderlineFormattedNodeMatcher =
            new RegexMatcher<MarkdownNode>(
                new Regex("_(__.+?__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
                (p, m) => new FormattedNode(TextFormatting.Italic,
                    Parse(p.Slice(m.Groups[1]), UnderlineFormattedNodeMatcher))
            );

        // Capture any character until the earliest double tilde
        private static readonly IMatcher<MarkdownNode> StrikethroughFormattedNodeMatcher =
            new RegexMatcher<MarkdownNode>(
                new Regex("~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
                (p, m) => new FormattedNode(TextFormatting.Strikethrough, Parse(p.Slice(m.Groups[1])))
            );

        // Capture any character until the earliest double pipe
        private static readonly IMatcher<MarkdownNode> SpoilerFormattedNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\|\\|(.+?)\\|\\|", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Spoiler, Parse(p.Slice(m.Groups[1])))
        );

        // Capture any character until the end of the line
        // Opening 'greater than' character must be followed by whitespace
        private static readonly IMatcher<MarkdownNode> SingleLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("^>\\s(.+\n?)", DefaultRegexOptions),
            (p, m) => new FormattedNode(TextFormatting.Quote, Parse(p.Slice(m.Groups[1])))
        );

        // Repeatedly capture any character until the end of the line
        // This one is tricky as it ends up producing multiple separate captures which need to be joined
        private static readonly IMatcher<MarkdownNode> RepeatedSingleLineQuoteNodeMatcher =
            new RegexMatcher<MarkdownNode>(
                new Regex("(?:^>\\s(.+\n?)){2,}", DefaultRegexOptions),
                (_, m) =>
                {
                    var content = string.Concat(m.Groups[1].Captures.Select(c => c.Value));
                    return new FormattedNode(TextFormatting.Quote, Parse(content));
                }
            );

        // Capture any character until the end of the input
        // Opening 'greater than' characters must be followed by whitespace
        private static readonly IMatcher<MarkdownNode> MultiLineQuoteNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("^>>>\\s(.+)", DefaultRegexOptions | RegexOptions.Singleline),
            (p, m) => new FormattedNode(TextFormatting.Quote, Parse(p.Slice(m.Groups[1])))
        );

        /* Code blocks */

        // Capture any character except backtick until a backtick
        // Blank lines at the beginning and end of content are trimmed
        // There can be either one or two backticks, but equal number on both sides
        private static readonly IMatcher<MarkdownNode> InlineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("(`{1,2})([^`]+)\\1", DefaultRegexOptions | RegexOptions.Singleline),
            m => new InlineCodeBlockNode(m.Groups[2].Value.Trim('\r', '\n'))
        );

        // Capture language identifier and then any character until the earliest triple backtick
        // Language identifier is one word immediately after opening backticks, followed immediately by newline
        // Blank lines at the beginning and end of content are trimmed
        private static readonly IMatcher<MarkdownNode> MultiLineCodeBlockNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("```(?:(\\w*)\\n)?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
            m => new MultiLineCodeBlockNode(m.Groups[1].Value, m.Groups[2].Value.Trim('\r', '\n'))
        );

        /* Mentions */

        // Capture @everyone
        private static readonly IMatcher<MarkdownNode> EveryoneMentionNodeMatcher = new StringMatcher<MarkdownNode>(
            "@everyone",
            _ => new MentionNode("everyone", MentionType.Meta)
        );

        // Capture @here
        private static readonly IMatcher<MarkdownNode> HereMentionNodeMatcher = new StringMatcher<MarkdownNode>(
            "@here",
            _ => new MentionNode("here", MentionType.Meta)
        );

        // Capture <@123456> or <@!123456>
        private static readonly IMatcher<MarkdownNode> UserMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("<@!?(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Groups[1].Value, MentionType.User)
        );

        // Capture <#123456>
        private static readonly IMatcher<MarkdownNode> ChannelMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("<#(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Groups[1].Value, MentionType.Channel)
        );

        // Capture <@&123456>
        private static readonly IMatcher<MarkdownNode> RoleMentionNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("<@&(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Groups[1].Value, MentionType.Role)
        );

        /* Emojis */

        // Capture any country flag emoji (two regional indicator surrogate pairs)
        // ... or "miscellaneous symbol" character
        // ... or surrogate pair
        // ... or digit followed by enclosing mark
        // (this does not match all emojis in Discord but it's reasonably accurate enough)
        private static readonly IMatcher<MarkdownNode> StandardEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("((?:[\\uD83C][\\uDDE6-\\uDDFF]){2}|[\\u2600-\\u26FF]|\\p{Cs}{2}|\\d\\p{Me})",
                DefaultRegexOptions),
            m => new EmojiNode(m.Groups[1].Value)
        );

        // Capture <:lul:123456> or <a:lul:123456>
        private static readonly IMatcher<MarkdownNode> CustomEmojiNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("<(a)?:(.+?):(\\d+?)>", DefaultRegexOptions),
            m => new EmojiNode(m.Groups[3].Value, m.Groups[2].Value, !string.IsNullOrWhiteSpace(m.Groups[1].Value))
        );

        /* Links */

        // Capture [title](link)
        private static readonly IMatcher<MarkdownNode> TitledLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\[(.+?)\\]\\((.+?)\\)", DefaultRegexOptions),
            m => new LinkNode(m.Groups[2].Value, m.Groups[1].Value)
        );

        // Capture any non-whitespace character after http:// or https:// until the last punctuation character or whitespace
        private static readonly IMatcher<MarkdownNode> AutoLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("(https?://\\S*[^\\.,:;\"\'\\s])", DefaultRegexOptions),
            m => new LinkNode(m.Groups[1].Value)
        );

        // Same as auto link but also surrounded by angular brackets
        private static readonly IMatcher<MarkdownNode> HiddenLinkNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("<(https?://\\S*[^\\.,:;\"\'\\s])>", DefaultRegexOptions),
            m => new LinkNode(m.Groups[1].Value)
        );

        /* Text */

        // Capture the shrug emoticon
        // This escapes it from matching for formatting
        private static readonly IMatcher<MarkdownNode> ShrugTextNodeMatcher = new StringMatcher<MarkdownNode>(
            @"¯\_(ツ)_/¯",
            p => new TextNode(p.ToString())
        );

        // Capture some specific emojis that don't get rendered
        // This escapes it from matching for emoji
        private static readonly IMatcher<MarkdownNode> IgnoredEmojiTextNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("(\\u26A7|\\u2640|\\u2642|\\u2695|\\u267E|\\u00A9|\\u00AE|\\u2122)", DefaultRegexOptions),
            m => new TextNode(m.Groups[1].Value)
        );

        // Capture any "symbol/other" character or surrogate pair preceded by a backslash
        // This escapes it from matching for emoji
        private static readonly IMatcher<MarkdownNode> EscapedSymbolTextNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\\\(\\p{So}|\\p{Cs}{2})", DefaultRegexOptions),
            m => new TextNode(m.Groups[1].Value)
        );

        // Capture any non-whitespace, non latin alphanumeric character preceded by a backslash
        // This escapes it from matching for formatting or other tokens
        private static readonly IMatcher<MarkdownNode> EscapedCharacterTextNodeMatcher = new RegexMatcher<MarkdownNode>(
            new Regex("\\\\([^a-zA-Z0-9\\s])", DefaultRegexOptions),
            m => new TextNode(m.Groups[1].Value)
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
            ItalicBoldFormattedNodeMatcher,
            ItalicUnderlineFormattedNodeMatcher,
            BoldFormattedNodeMatcher,
            ItalicFormattedNodeMatcher,
            UnderlineFormattedNodeMatcher,
            ItalicAltFormattedNodeMatcher,
            StrikethroughFormattedNodeMatcher,
            SpoilerFormattedNodeMatcher,
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
            CustomEmojiNodeMatcher
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
            CustomEmojiNodeMatcher
        );

        private static IReadOnlyList<MarkdownNode> Parse(StringPart stringPart, IMatcher<MarkdownNode> matcher) =>
            matcher
                .MatchAll(stringPart, p => new TextNode(p.ToString()))
                .Select(r => r.Value)
                .ToArray();
    }

    internal static partial class MarkdownParser
    {
        private static IReadOnlyList<MarkdownNode> Parse(StringPart stringPart) => Parse(stringPart, AggregateNodeMatcher);

        private static IReadOnlyList<MarkdownNode> ParseMinimal(StringPart stringPart) => Parse(stringPart, MinimalAggregateNodeMatcher);

        public static IReadOnlyList<MarkdownNode> Parse(string input) => Parse(new StringPart(input));

        public static IReadOnlyList<MarkdownNode> ParseMinimal(string input) => ParseMinimal(new StringPart(input));
    }
}