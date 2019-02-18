using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal static class MarkdownGrammar
    {
        private const string BoldToken = "**";
        private const string ItalicToken = "*";
        private const string ItalicAltToken = "_";
        private const string UnderlineToken = "__";
        private const string StrikethroughToken = "~~";
        private const string SpoilerToken = "||";
        private const string InlineCodeBlockToken = "`";
        private const string MultilineCodeBlockToken = "```";

        private static Parser<string> NodeContent(string token) =>
            Parse.RegexMatch(
                $"{Regex.Escape(token)}" + // open token
                "(.+?)" + // any non-empty content inside
                $"{Regex.Escape(token)}" + // close token
                $"(?=[^{Regex.Escape(token.Last().ToString())}]|$)") // followed by a different char than last or EOL
                .Select(m => m.Groups[1].Value) // get 1st group value
                .Where(s => s.IsNotBlank()); // non-blank results

        private static readonly Parser<Node> BoldFormattedNode =
            NodeContent(BoldToken).Select(s => new FormattedNode(TextFormatting.Bold, BuildTree(s)));

        private static readonly Parser<Node> ItalicFormattedNode =
            NodeContent(ItalicToken).Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        private static readonly Parser<Node> ItalicAltFormattedNode =
            NodeContent(ItalicAltToken).Select(s => new FormattedNode(TextFormatting.Italic, BuildTree(s)));

        private static readonly Parser<Node> UnderlineFormattedNode =
            NodeContent(UnderlineToken).Select(s => new FormattedNode(TextFormatting.Underline, BuildTree(s)));

        private static readonly Parser<Node> StrikethroughFormattedNode =
            NodeContent(StrikethroughToken).Select(s => new FormattedNode(TextFormatting.Strikethrough, BuildTree(s)));

        private static readonly Parser<Node> SpoilerFormattedNode =
            NodeContent(SpoilerToken).Select(s => new FormattedNode(TextFormatting.Spoiler, BuildTree(s)));

        private static readonly Parser<Node> InlineCodeBlockNode =
            NodeContent(InlineCodeBlockToken).Select(s => new InlineCodeBlockNode(s));

        private static readonly Parser<Node> MultilineCodeBlockNode =
            NodeContent(MultilineCodeBlockToken).Select(s => new InlineCodeBlockNode(s)); // temporary

        private static readonly Parser<Node> FunctionalNode = BoldFormattedNode
            .Or(UnderlineFormattedNode).Or(ItalicFormattedNode).Or(ItalicAltFormattedNode)
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode).Or(InlineCodeBlockNode)
            .Or(MultilineCodeBlockNode);

        private static readonly Parser<Node> TextNode =
            Parse.AnyChar.Except(FunctionalNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        private static readonly Parser<Node> AnyNode = FunctionalNode.Or(TextNode);

        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}