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
        // Parses the text contained within two instances of the given token
        private static Parser<string> ContentWithinTokens(string token) =>
            Parse.RegexMatch(
                $"{Regex.Escape(token)}" + // open token
                "(.+?)" + // any non-empty content inside
                $"{Regex.Escape(token)}" + // close token
                $"(?=[^{Regex.Escape(token.Last().ToString())}]|$)") // followed by a different char than last or EOL
                .Select(m => m.Groups[1].Value) // get 1st group value
                .Where(s => s.IsNotBlank()); // non-blank results

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

        /* Code blocks */

        private static readonly Parser<Node> InlineCodeBlockNode =
            ContentWithinTokens("`").Select(s => new InlineCodeBlockNode(s));

        private static readonly Parser<Node> MultilineCodeBlockNode =
            ContentWithinTokens("```").Select(s => new InlineCodeBlockNode(s)); // temporary

        // Functional node is any node apart from text
        // It's important that italic goes after bold and underline due to token conflicts
        private static readonly Parser<Node> FunctionalNode = BoldFormattedNode
            .Or(UnderlineFormattedNode).Or(ItalicFormattedNode).Or(ItalicAltFormattedNode)
            .Or(StrikethroughFormattedNode).Or(SpoilerFormattedNode)
            .Or(InlineCodeBlockNode).Or(MultilineCodeBlockNode);

        // Text node is created for text spans that don't match with any functional nodes
        private static readonly Parser<Node> TextNode =
            Parse.AnyChar.Except(FunctionalNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        // Any node
        private static readonly Parser<Node> AnyNode = FunctionalNode.Or(TextNode);

        // Entry point
        public static IReadOnlyList<Node> BuildTree(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}