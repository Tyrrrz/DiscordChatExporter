using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    // The following parsing logic is intentionally built in such way to make it resemble Discord's own
    // parsing as closely as possible, including its numerous flaws and inconsistencies
    internal static class Grammar
    {
        // TODO: escape char \

        /* Formatting */

        // May contain italics inside, so capture until the last double asterisk that isn't followed by an asterisk
        private static readonly Parser<Node> BoldFormattedNode =
            Parse.RegexMatch(new Regex("\\*\\*(.+?)\\*\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Bold, BuildTree(s)));

        // TODO: *asd**
        // TODO: ***s**a**b***
        // TODO: one *two **three** __four__* _five_ ***six***
        // Cannot have whitespace right after opening or right before closing token
        private static readonly Parser<Node> ItalicFormattedNode =
            Parse.RegexMatch(new Regex("\\*(?!\\s)(.+?)(?<!\\s)\\*(?!\\*)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        // Opening/closing token cannot come after/before word characters
        private static readonly Parser<Node> ItalicAltFormattedNode =
            Parse.RegexMatch(new Regex("(?<!\\w)_(.+?)_(?!\\w)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        // May contain italics inside, so capture until the last double underline that isn't followed by an underline
        private static readonly Parser<Node> UnderlineFormattedNode =
            Parse.RegexMatch(new Regex("__(.+?)__(?!_)", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Underline, BuildTree(s)));

        // Strikethrough is safe
        private static readonly Parser<Node> StrikethroughFormattedNode =
            Parse.RegexMatch(new Regex("~~(.+?)~~", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Strikethrough, BuildTree(s)));

        // Spoiler is safe
        private static readonly Parser<Node> SpoilerFormattedNode =
            Parse.RegexMatch(new Regex("\\|\\|(.+?)\\|\\|", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new FormattedNode(TextFormatting.Spoiler, BuildTree(s)));

        private static readonly Parser<Node> AnyFormattedNode = BoldFormattedNode
            .Or(UnderlineFormattedNode).Or(ItalicFormattedNode).Or(ItalicAltFormattedNode) // italic should be after bold & underline
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode);

        /* Code blocks */

        // Can't have backticks inside and surrounding whitespace is trimmed
        private static readonly Parser<Node> InlineCodeBlockNode =
            Parse.RegexMatch(new Regex("`\\s*([^`]+?)\\s*`", RegexOptions.Singleline))
                .Select(m => m.Groups[1].Value)
                .Select(s => new InlineCodeBlockNode(s));

        // The first word is a language identifier if it's the only word followed by a newline, the rest is code
        private static readonly Parser<Node> MultilineCodeBlockNode =
            Parse.RegexMatch(new Regex("```(?:(\\S*?)?(?:\\s*?\\n))?(.+)```", RegexOptions.Singleline))
                .Select(m => new MultilineCodeBlockNode(m.Groups[1].Value, m.Groups[2].Value));

        private static readonly Parser<Node> AnyCodeBlockNode = MultilineCodeBlockNode.Or(InlineCodeBlockNode); // inline should be after multiline

        /* Mentions */

        // TODO: check boundaries
        private static readonly Parser<Node> EveryoneMentionNode = Parse.RegexMatch("(?<=\\s|^)@everyone(?=\\s|$)")
            .Select(s => new MentionNode("everyone", MentionType.Meta));

        // TODO: check boundaries
        private static readonly Parser<Node> HereMentionNode = Parse.RegexMatch("(?<=\\s|^)@here(?=\\s|$)")
            .Select(s => new MentionNode("here", MentionType.Meta));

        private static readonly Parser<Node> UserMentionNode = Parse.RegexMatch("<@!?(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.User));

        private static readonly Parser<Node> ChannelMentionNode = Parse.RegexMatch("<#(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.Channel));

        private static readonly Parser<Node> RoleMentionNode = Parse.RegexMatch("<@&(\\d+)>")
            .Select(m => m.Groups[1].Value).Select(s => new MentionNode(s, MentionType.Role));

        private static readonly Parser<Node> AnyMentionNode = EveryoneMentionNode.Or(HereMentionNode)
            .Or(UserMentionNode).Or(ChannelMentionNode).Or(RoleMentionNode);

        /* Emojis */

        private static readonly Parser<Node> EmojiNode = Parse.RegexMatch("<:(.+):(\\d+)>")
            .Select(m => new EmojiNode(m.Groups[2].Value, m.Groups[1].Value));

        private static readonly Parser<Node> AnyEmojiNode = EmojiNode;

        /* Links */

        // TODO: check boundaries
        private static readonly Parser<Node> TitledLinkNode = Parse.RegexMatch("\\[(.+)\\]\\((.+)\\)")
            .Select(m => new LinkNode(m.Groups[2].Value, m.Groups[1].Value));

        // TODO: check boundaries
        private static readonly Parser<Node> AutoLinkNode = Parse.RegexMatch("(https?://.*?)(?=\\s|$)")
            .Select(m => m.Groups[1].Value).Select(s => new LinkNode(s));

        private static readonly Parser<Node> AnyLinkNode = TitledLinkNode.Or(AutoLinkNode);

        /* Functional (everything above) */

        private static readonly Parser<Node> AnyFunctionalNode =
            AnyFormattedNode.Or(AnyCodeBlockNode).Or(AnyMentionNode).Or(AnyEmojiNode).Or(AnyLinkNode);

        /* Text */

        // Shrug is an exception and doesn't get formatted
        private static readonly Parser<Node> ShrugTextNode =
            Parse.String("¯\\_(ツ)_/¯").Text().Select(s => new TextNode(s));

        // Fallback text node is for any node that is not functional
        private static readonly Parser<Node> FallbackTextNode =
            Parse.AnyChar.Except(AnyFunctionalNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        private static readonly Parser<Node> AnyTextNode = ShrugTextNode.Or(FallbackTextNode);

        // Any node
        private static readonly Parser<Node> AnyNode = AnyTextNode.Or(AnyFunctionalNode); // functional should be after text

        // Entry point
        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}