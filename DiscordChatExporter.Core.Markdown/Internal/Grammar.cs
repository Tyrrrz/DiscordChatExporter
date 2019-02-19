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
        // Parses the text contained within two instances of the given token
        private static Parser<string> ContentWithinTokens(string token) =>
            Parse.RegexMatch(
                $"{Regex.Escape(token)}" + // open token
                "(.+?)" + // any non-empty content inside
                $"{Regex.Escape(token)}" + // close token
                $"(?=[^{Regex.Escape(token.Last().ToString())}]|$)") // followed by a different char than last or EOL
                .Select(m => m.Groups[1].Value); // get 1st group value

        /* Formatting */

        private static readonly Parser<Node> BoldFormattedNode =
            ContentWithinTokens("**").Select(s => new FormattedNode(TextFormatting.Bold, BuildTree(s)));

        private static readonly Parser<Node> ItalicFormattedNode =
            ContentWithinTokens("*").Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        private static readonly Parser<Node> ItalicAltFormattedNode =
            ContentWithinTokens("_").Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        private static readonly Parser<Node> UnderlineFormattedNode =
            ContentWithinTokens("__").Select(s => new FormattedNode(TextFormatting.Underline, BuildTree(s)));

        private static readonly Parser<Node> StrikethroughFormattedNode =
            ContentWithinTokens("~~").Select(s => new FormattedNode(TextFormatting.Strikethrough, BuildTree(s)));

        private static readonly Parser<Node> SpoilerFormattedNode =
            ContentWithinTokens("||").Select(s => new FormattedNode(TextFormatting.Spoiler, BuildTree(s)));

        private static readonly Parser<Node> AnyFormattedNode = BoldFormattedNode
            .Or(UnderlineFormattedNode).Or(ItalicFormattedNode).Or(ItalicAltFormattedNode) // italic should be after bold & underline
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode);

        /* Code blocks */

        private static readonly Parser<Node> InlineCodeBlockNode =
            ContentWithinTokens("`").Select(s => new InlineCodeBlockNode(s));

        private static readonly Parser<Node> MultilineCodeBlockNode =
            ContentWithinTokens("```").Select(s => new InlineCodeBlockNode(s)); // temporary

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

        /* Links */

        // TODO: check boundaries
        private static readonly Parser<Node> TitledLinkNode = Parse.RegexMatch("\\[(.+)\\]\\((.+)\\)")
            .Select(m => new LinkNode(m.Groups[2].Value, m.Groups[1].Value));

        // TODO: check boundaries
        private static readonly Parser<Node> AutoLinkNode = Parse.RegexMatch("(https?://.*?)(?=\\s|$)")
            .Select(m => m.Groups[1].Value).Select(s => new LinkNode(s));

        private static readonly Parser<Node> AnyLinkNode = TitledLinkNode.Or(AutoLinkNode);

        // Functional node is any node apart from plain text
        private static readonly Parser<Node> FunctionalNode =
            AnyFormattedNode.Or(AnyCodeBlockNode).Or(AnyMentionNode).Or(EmojiNode).Or(AnyLinkNode);

        // Text node is created for text spans that don't match with any functional nodes
        private static readonly Parser<Node> TextNode =
            Parse.AnyChar.Except(FunctionalNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        // Any node
        private static readonly Parser<Node> AnyNode = FunctionalNode.Or(TextNode);

        // Entry point
        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}