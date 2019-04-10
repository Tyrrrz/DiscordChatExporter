using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Markdown.Internal;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown
{
    // The following parsing logic is meant to replicate Discord's markdown grammar as close as possible
    public static class MarkdownParser
    {
        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /* Formatting */

        // Capture any character until the earliest double asterisk not followed by an asterisk
        private static readonly IMatcher<Node> BoldFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "**", TextFormatting.Bold, Parse(m.Groups[1].Value)));

        // Capture any character until the earliest single asterisk not preceded or followed by an asterisk
        // Opening asterisk must not be followed by whitespace
        // Closing asterisk must not be preceeded by whitespace
        private static readonly IMatcher<Node> ItalicFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture any character until the earliest triple asterisk not followed by an asterisk
        private static readonly IMatcher<Node> ItalicBoldFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\*(\\*\\*.+?\\*\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value, BoldFormattedNodeMatcher)));

        // Capture any character except underscore until an underscore
        // Closing underscore must not be followed by a word character
        private static readonly IMatcher<Node> ItalicAltFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("_([^_]+)_(?!\\w)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture any character until the earliest double underscore not followed by an underscore
        private static readonly IMatcher<Node> UnderlineFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "__", TextFormatting.Underline, Parse(m.Groups[1].Value)));

        // Capture any character until the earliest triple underscore not followed by an underscore
        private static readonly IMatcher<Node> ItalicUnderlineFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("_(__.+?__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value, UnderlineFormattedNodeMatcher)));

        // Capture any character until the earliest double tilde
        private static readonly IMatcher<Node> StrikethroughFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "~~", TextFormatting.Strikethrough, Parse(m.Groups[1].Value)));

        // Capture any character until the earliest double pipe
        private static readonly IMatcher<Node> SpoilerFormattedNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\|\\|(.+?)\\|\\|", DefaultRegexOptions | RegexOptions.Singleline),
            m => new FormattedNode(m.Value, "||", TextFormatting.Spoiler, Parse(m.Groups[1].Value)));

        /* Code blocks */

        // Capture any character except backtick until a backtick
        // Whitespace surrounding content inside backticks is trimmed
        private static readonly IMatcher<Node> InlineCodeBlockNodeMatcher = new RegexMatcher<Node>(
            new Regex("`([^`]+)`", DefaultRegexOptions | RegexOptions.Singleline),
            m => new InlineCodeBlockNode(m.Value, m.Groups[1].Value.Trim()));

        // Capture language identifier and then any character until the earliest triple backtick
        // Languge identifier is one word immediately after opening backticks, followed immediately by newline
        // Whitespace surrounding content inside backticks is trimmed
        private static readonly IMatcher<Node> MultilineCodeBlockNodeMatcher = new RegexMatcher<Node>(
            new Regex("```(?:(\\w*)\\n)?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
            m => new MultilineCodeBlockNode(m.Value, m.Groups[1].Value, m.Groups[2].Value.Trim()));

        /* Mentions */

        // Capture @everyone
        private static readonly IMatcher<Node> EveryoneMentionNodeMatcher = new StringMatcher<Node>(
            "@everyone",
            s => new MentionNode(s, "everyone", MentionType.Meta));

        // Capture @here
        private static readonly IMatcher<Node> HereMentionNodeMatcher = new StringMatcher<Node>(
            "@here",
            s => new MentionNode(s, "here", MentionType.Meta));

        // Capture <@123456> or <@!123456>
        private static readonly IMatcher<Node> UserMentionNodeMatcher = new RegexMatcher<Node>(
            new Regex("<@!?(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.User));

        // Capture <#123456>
        private static readonly IMatcher<Node> ChannelMentionNodeMatcher = new RegexMatcher<Node>(
            new Regex("<#(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Channel));

        // Capture <@&123456>
        private static readonly IMatcher<Node> RoleMentionNodeMatcher = new RegexMatcher<Node>(
            new Regex("<@&(\\d+)>", DefaultRegexOptions),
            m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Role));

        /* Emojis */

        // Capture any country flag emoji (two regional indicator surrogate pairs)
        // ... or "symbol/other" character
        // ... or surrogate pair
        // ... or digit followed by enclosing mark
        // (this does not match all emojis in Discord but it's reasonably accurate enough)
        private static readonly IMatcher<Node> StandardEmojiNodeMatcher = new RegexMatcher<Node>(
            new Regex("((?:[\\uD83C][\\uDDE6-\\uDDFF]){2}|\\p{So}|\\p{Cs}{2}|\\d\\p{Me})", DefaultRegexOptions),
            m => new EmojiNode(m.Value, m.Groups[1].Value));

        // Capture <:lul:123456> or <a:lul:123456>
        private static readonly IMatcher<Node> CustomEmojiNodeMatcher = new RegexMatcher<Node>(
            new Regex("<(a)?:(.+?):(\\d+?)>", DefaultRegexOptions),
            m => new EmojiNode(m.Value, m.Groups[3].Value, m.Groups[2].Value, !m.Groups[1].Value.IsEmpty()));

        /* Links */

        // Capture [title](link)
        private static readonly IMatcher<Node> TitledLinkNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\[(.+?)\\]\\((.+?)\\)", DefaultRegexOptions),
            m => new LinkNode(m.Value, m.Groups[2].Value, m.Groups[1].Value));

        // Capture any non-whitespace character after http:// or https:// until the last punctuation character or whitespace
        private static readonly IMatcher<Node> AutoLinkNodeMatcher = new RegexMatcher<Node>(
            new Regex("(https?://\\S*[^\\.,:;\"\'\\s])", DefaultRegexOptions),
            m => new LinkNode(m.Value, m.Groups[1].Value));

        // Same as auto link but also surrounded by angular brackets
        private static readonly IMatcher<Node> HiddenLinkNodeMatcher = new RegexMatcher<Node>(
            new Regex("<(https?://\\S*[^\\.,:;\"\'\\s])>", DefaultRegexOptions),
            m => new LinkNode(m.Value, m.Groups[1].Value));

        /* Text */

        // Capture the shrug emoticon
        // This escapes it from matching for formatting
        private static readonly IMatcher<Node> ShrugTextNodeMatcher = new StringMatcher<Node>(
            @"¯\_(ツ)_/¯",
            s => new TextNode(s));

        // Capture any "symbol/other" character or surrogate pair preceeded by a backslash
        // This escapes it from matching for emoji
        private static readonly IMatcher<Node> EscapedSymbolTextNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\\\(\\p{So}|\\p{Cs}{2})", DefaultRegexOptions),
            m => new TextNode(m.Value, m.Groups[1].Value));

        // Capture any non-whitespace, non latin alphanumeric character preceeded by a backslash
        // This escapes it from matching for formatting or other tokens
        private static readonly IMatcher<Node> EscapedCharacterTextNodeMatcher = new RegexMatcher<Node>(
            new Regex("\\\\([^a-zA-Z0-9\\s])", DefaultRegexOptions),
            m => new TextNode(m.Value, m.Groups[1].Value));

        // Combine all matchers into one
        // Matchers that have similar patterns are ordered from most specific to least specific
        private static readonly IMatcher<Node> AggregateNodeMatcher = new AggregateMatcher<Node>(
            ItalicBoldFormattedNodeMatcher,
            ItalicUnderlineFormattedNodeMatcher,
            BoldFormattedNodeMatcher,
            ItalicFormattedNodeMatcher,
            UnderlineFormattedNodeMatcher,
            ItalicAltFormattedNodeMatcher,
            StrikethroughFormattedNodeMatcher,
            SpoilerFormattedNodeMatcher,
            MultilineCodeBlockNodeMatcher,
            InlineCodeBlockNodeMatcher,
            EveryoneMentionNodeMatcher,
            HereMentionNodeMatcher,
            UserMentionNodeMatcher,
            ChannelMentionNodeMatcher,
            RoleMentionNodeMatcher,
            StandardEmojiNodeMatcher,
            CustomEmojiNodeMatcher,
            TitledLinkNodeMatcher,
            AutoLinkNodeMatcher,
            HiddenLinkNodeMatcher,
            ShrugTextNodeMatcher,
            EscapedSymbolTextNodeMatcher,
            EscapedCharacterTextNodeMatcher);

        private static IReadOnlyList<Node> Parse(string input, IMatcher<Node> matcher) =>
            matcher.MatchAll(input, s => new TextNode(s)).Select(r => r.Value).ToArray();

        public static IReadOnlyList<Node> Parse(string input) => Parse(input, AggregateNodeMatcher);
    }
}