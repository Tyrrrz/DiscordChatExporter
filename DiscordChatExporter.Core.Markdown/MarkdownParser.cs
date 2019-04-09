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

        // Capture until the earliest double asterisk not followed by an asterisk
        private static readonly IMatcher<Node> BoldFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "**", TextFormatting.Bold, Parse(m.Groups[1].Value)));

        // Capture until the earliest single asterisk not preceded or followed by an asterisk
        // Can't have whitespace right after opening or right before closing asterisk
        private static readonly IMatcher<Node> ItalicFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture until the earliest underscore
        // Can't have underscores inside
        // Can't have word characters right after closing underscore
        private static readonly IMatcher<Node> ItalicAltFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("_([^_]+?)_(?!\\w)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple asterisk not preceded or followed by an asterisk
        private static readonly IMatcher<Node> ItalicBoldFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\*(\\*\\*(?:.+?)\\*\\*)\\*(?!\\*)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "*", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture until the earliest double underscore not followed by an underscore
        private static readonly IMatcher<Node> UnderlineFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("__(.+?)__(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "__", TextFormatting.Underline, Parse(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple underscore not preceded or followed by an underscore
        private static readonly IMatcher<Node> ItalicUnderlineFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("_(__(?:.+?)__)_(?!_)", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "_", TextFormatting.Italic, Parse(m.Groups[1].Value)));

        // Capture until the earliest double tilde
        private static readonly IMatcher<Node> StrikethroughFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("~~(.+?)~~", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "~~", TextFormatting.Strikethrough, Parse(m.Groups[1].Value)));

        // Capture until the earliest double pipe
        private static readonly IMatcher<Node> SpoilerFormattedNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\|\\|(.+?)\\|\\|", DefaultRegexOptions | RegexOptions.Singleline),
                m => new FormattedNode(m.Value, "||", TextFormatting.Spoiler, Parse(m.Groups[1].Value)));

        /* Code blocks */

        // Capture until the earliest backtick
        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly IMatcher<Node> InlineCodeBlockNodeMatcher =
            new RegexMatcher<Node>(new Regex("`([^`]+?)`", DefaultRegexOptions | RegexOptions.Singleline),
                m => new InlineCodeBlockNode(m.Value, m.Groups[1].Value.Trim()));

        // Capture language identifier and then everything until the earliest triple backtick
        // Languge identifier is one word immediately after opening backticks, followed immediately by newline
        private static readonly IMatcher<Node> MultilineCodeBlockNodeMatcher =
            new RegexMatcher<Node>(new Regex("```(?:(\\w*)\\n)?(.+?)```", DefaultRegexOptions | RegexOptions.Singleline),
                m => new MultilineCodeBlockNode(m.Value, m.Groups[1].Value, m.Groups[2].Value));

        /* Mentions */

        // @everyone
        private static readonly IMatcher<Node> EveryoneMentionNodeMatcher =
            new StringMatcher<Node>("@everyone", s => new MentionNode(s, s.Substring(1), MentionType.Meta));

        // @here
        private static readonly IMatcher<Node> HereMentionNodeMatcher =
            new StringMatcher<Node>("@here", s => new MentionNode(s, s.Substring(1), MentionType.Meta));

        // <@123456> or <@!123456>
        private static readonly IMatcher<Node> UserMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<@!?(\\d+?)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.User));

        // <#123456>
        private static readonly IMatcher<Node> ChannelMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<#(\\d+?)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Channel));

        // <@&123456>
        private static readonly IMatcher<Node> RoleMentionNodeMatcher =
            new RegexMatcher<Node>(new Regex("<@&(\\d+?)>", DefaultRegexOptions),
                m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Role));

        /* Emojis */

        // Matches all standard unicode emojis
        private static readonly IMatcher<Node> StandardEmojiNodeMatcher =
            new RegexMatcher<Node>(new Regex("(\\p{So}|\\p{Cs}+|\\d\\p{Me})", DefaultRegexOptions),
                m => new EmojiNode(m.Value, m.Groups[1].Value));

        // <:lul:123456> or <a:lul:123456>
        private static readonly IMatcher<Node> CustomEmojiNodeMatcher =
            new RegexMatcher<Node>(new Regex("<(a)?:(.+?):(\\d+?)>", DefaultRegexOptions),
                m => new EmojiNode(m.Value, m.Groups[3].Value, m.Groups[2].Value, !m.Groups[1].Value.IsEmpty()));

        /* Links */

        // [title](link)
        private static readonly IMatcher<Node> TitledLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\[(.+?)\\]\\((.+?)\\)", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[2].Value, m.Groups[1].Value));

        // Starts with http:// or https://, stops at the last non-whitespace character followed by whitespace or punctuation character
        private static readonly IMatcher<Node> AutoLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("(https?://\\S*[^\\.,:;\"\'\\s])", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[1].Value));

        // Autolink surrounded by angular brackets
        private static readonly IMatcher<Node> HiddenLinkNodeMatcher =
            new RegexMatcher<Node>(new Regex("<(https?://\\S*[^\\.,:;\"\'\\s])>", DefaultRegexOptions),
                m => new LinkNode(m.Value, m.Groups[1].Value));

        /* Text */

        // Shrug is an exception and needs to be exempt from formatting
        private static readonly IMatcher<Node> ShrugTextNodeMatcher = new StringMatcher<Node>(@"¯\_(ツ)_/¯", s => new TextNode(s));

        // Backslash escapes any following unicode surrogate pair
        private static readonly IMatcher<Node> EscapedSurrogatePairTextNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\\\(\\p{Cs}{1,2})", DefaultRegexOptions),
                m => new TextNode(m.Value, m.Groups[1].Value));

        // Backslash escapes any following non-whitespace character except for digits and latin letters
        private static readonly IMatcher<Node> EscapedCharacterTextNodeMatcher =
            new RegexMatcher<Node>(new Regex("\\\\([^a-zA-Z0-9\\s])", DefaultRegexOptions),
                m => new TextNode(m.Value, m.Groups[1].Value));

        // Combine all matchers into one
        private static readonly IMatcher<Node> NodeMatcher = new AggregateMatcher<Node>(
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
            EscapedSurrogatePairTextNodeMatcher,
            EscapedCharacterTextNodeMatcher);

        public static IReadOnlyList<Node> Parse(string input) => NodeMatcher.MatchAll(input, s => new TextNode(s)).Select(r => r.Value).ToArray();
    }
}