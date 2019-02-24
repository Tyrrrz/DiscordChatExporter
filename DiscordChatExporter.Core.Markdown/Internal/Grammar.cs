using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    // The following parsing logic is intentionally built in such way to make it resemble Discord's own
    // parsing as closely as possible, including its numerous flaws and inconsistencies
    internal static class Grammar
    {
        /* Formatting */

        // Capture until the earliest double asterisk not followed by an asterisk
        private static readonly Parser<Node> BoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "**", TextFormatting.Bold, BuildTree(m.Groups[1].Value)));

        // Capture until the earliest single asterisk not preceded or followed by an asterisk
        // Can't have whitespace right after opening or right before closing asterisk
        private static readonly Parser<Node> ItalicFormattedNode =
            Parse.RegexMatch(new Regex("\\*(?!\\s)(.+?)(?<!\\s|\\*)\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "*", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Can't have underscores inside
        // Can't have word characters right after closing underscore
        private static readonly Parser<Node> ItalicAltFormattedNode =
            Parse.RegexMatch(new Regex("_([^_]+?)_(?!\\w)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "_", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple asterisk not preceded or followed by an asterisk
        private static readonly Parser<Node> ItalicBoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*(\\*\\*(?:.+?)\\*\\*)\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "*", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Capture until the earliest double underscore not followed by an underscore
        private static readonly Parser<Node> UnderlineFormattedNode =
            Parse.RegexMatch(new Regex("__(.+?)__(?!_)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "__", TextFormatting.Underline, BuildTree(m.Groups[1].Value)));

        // Treated as a separate entity for simplicity
        // Capture until the earliest triple underscore not preceded or followed by an underscore
        private static readonly Parser<Node> ItalicUnderlineFormattedNode =
            Parse.RegexMatch(new Regex("_(__(?:.+?)__)_(?!_)", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "_", TextFormatting.Italic, BuildTree(m.Groups[1].Value)));

        // Strikethrough is safe
        private static readonly Parser<Node> StrikethroughFormattedNode =
            Parse.RegexMatch(new Regex("~~(.+?)~~", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "~~", TextFormatting.Strikethrough, BuildTree(m.Groups[1].Value)));

        // Spoiler is safe
        private static readonly Parser<Node> SpoilerFormattedNode =
            Parse.RegexMatch(new Regex("\\|\\|(.+?)\\|\\|", RegexOptions.Singleline))
                .Select(m => new FormattedNode(m.Value, "||", TextFormatting.Spoiler, BuildTree(m.Groups[1].Value)));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyFormattedNode = 
            ItalicBoldFormattedNode.Or(ItalicUnderlineFormattedNode)
            .Or(BoldFormattedNode).Or(ItalicFormattedNode)
            .Or(UnderlineFormattedNode).Or(ItalicAltFormattedNode)
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode);

        /* Code blocks */

        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly Parser<Node> InlineCodeBlockNode =
            Parse.RegexMatch(new Regex("`\\s*([^`]+?)\\s*`", RegexOptions.Singleline))
                .Select(m => new InlineCodeBlockNode(m.Value, m.Groups[1].Value));

        // The first word is a language identifier if it's the only word followed by a newline, the rest is code
        private static readonly Parser<Node> MultilineCodeBlockNode =
            Parse.RegexMatch(new Regex("```(?:(\\w*?)?(?:\\s*?\\n))?(.+)```", RegexOptions.Singleline))
                .Select(m => new MultilineCodeBlockNode(m.Value, m.Groups[1].Value, m.Groups[2].Value));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyCodeBlockNode = MultilineCodeBlockNode.Or(InlineCodeBlockNode);

        /* Mentions */

        // @everyone or @here
        private static readonly Parser<Node> MetaMentionNode = Parse.RegexMatch("@(everyone|here)")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Meta));

        // <@123456> or <@!123456>
        private static readonly Parser<Node> UserMentionNode = Parse.RegexMatch("<@!?(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.User));

        // <#123456>
        private static readonly Parser<Node> ChannelMentionNode = Parse.RegexMatch("<#(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Channel));

        // <@&123456>
        private static readonly Parser<Node> RoleMentionNode = Parse.RegexMatch("<@&(\\d+)>")
            .Select(m => new MentionNode(m.Value, m.Groups[1].Value, MentionType.Role));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyMentionNode =
            MetaMentionNode.Or(UserMentionNode).Or(ChannelMentionNode).Or(RoleMentionNode);

        /* Emojis */

        // <:lul:123456> or <a:lul:123456>
        private static readonly Parser<Node> EmojiNode = Parse.RegexMatch("<(a)?:(.+):(\\d+)>")
            .Select(m => new EmojiNode(m.Value, m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value.IsNotBlank()));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyEmojiNode = EmojiNode;

        /* Links */

        // [title](link)
        private static readonly Parser<Node> TitledLinkNode = Parse.RegexMatch("\\[(.+)\\]\\((.+)\\)")
            .Select(m => new LinkNode(m.Value, m.Groups[2].Value, m.Groups[1].Value));

        // Starts with http:// or https://, stops at the last non-whitespace character followed by whitespace or punctuation character
        private static readonly Parser<Node> AutoLinkNode = Parse.RegexMatch("(https?://\\S*[^\\.,:;\"\'\\s])")
            .Select(m => new LinkNode(m.Value, m.Groups[1].Value));

        // Autolink surrounded by angular brackets
        private static readonly Parser<Node> HiddenLinkNode = Parse.RegexMatch("<(https?://\\S*[^\\.,:;\"\'\\s])>")
            .Select(m => new LinkNode(m.Value, m.Groups[1].Value));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyLinkNode = TitledLinkNode.Or(HiddenLinkNode).Or(AutoLinkNode); 

        /* Text */

        // Shrug is an exception and needs to be exempt from formatting
        private static readonly Parser<Node> ShrugTextNode =
            Parse.String("¯\\_(ツ)_/¯").Text().Select(s => new TextNode(s));

        // Backslash escapes any following non-whitespace character except for digits and latin letters
        private static readonly Parser<Node> EscapedTextNode =
            Parse.RegexMatch("\\\\([^a-zA-Z0-9\\s])").Select(m => new TextNode(m.Value, m.Groups[1].Value));

        // Aggregator, order matters
        private static readonly Parser<Node> AnyTextNode = ShrugTextNode.Or(EscapedTextNode);

        /* Aggregator and fallback */

        // Any node recognized by above patterns
        private static readonly Parser<Node> AnyRecognizedNode = AnyFormattedNode.Or(AnyCodeBlockNode)
            .Or(AnyMentionNode).Or(AnyEmojiNode).Or(AnyLinkNode).Or(AnyTextNode);

        // Any node not recognized by above patterns (treated as plain text)
        private static readonly Parser<Node> FallbackNode =
            Parse.AnyChar.Except(AnyRecognizedNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        // Any node
        private static readonly Parser<Node> AnyNode = AnyRecognizedNode.Or(FallbackNode);

        // Entry point
        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}