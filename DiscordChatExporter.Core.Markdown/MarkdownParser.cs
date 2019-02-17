using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace DiscordChatExporter.Core.Markdown
{
    public class MarkdownParser
    {
        private static readonly Parser<string> BoldToken = Parse.String("**").Text();
        private static readonly Parser<string> ItalicToken = Parse.String("*").Text();
        private static readonly Parser<string> ItalicAltToken = Parse.String("_").Text();
        private static readonly Parser<string> UnderlineToken = Parse.String("__").Text();
        private static readonly Parser<string> StrikethroughToken = Parse.String("~~").Text();
        private static readonly Parser<string> InlineCodeBlockToken = Parse.String("`").Text();
        private static readonly Parser<string> MultilineCodeBlockToken = Parse.String("```").Text();
        private static readonly Parser<string> SpoilerToken = Parse.String("||").Text();

        private static readonly Parser<string> AnyToken = BoldToken
            .Or(UnderlineToken)
            .Or(ItalicToken)
            .Or(ItalicAltToken)
            .Or(StrikethroughToken);

        private static readonly Parser<Node> BoldFormattedContainerNode =
            from open in BoldToken
            from children in Parse.Ref(() => AnyNode).AtLeastOnce()
            from close in BoldToken
            select new FormattedContainerNode(TextFormatting.Bold, children.ToArray());

        private static readonly Parser<Node> ItalicFormattedContainerNode =
            from open in ItalicToken
            from children in Parse.Ref(() => AnyNode).AtLeastOnce()
            from close in ItalicToken
            select new FormattedContainerNode(TextFormatting.Italic, children.ToArray());

        private static readonly Parser<Node> ItalicAltFormattedContainerNode =
            from open in ItalicAltToken
            from children in Parse.Ref(() => AnyNode).AtLeastOnce()
            from close in ItalicAltToken
            select new FormattedContainerNode(TextFormatting.Italic, children.ToArray());

        private static readonly Parser<Node> UnderlineFormattedContainerNode =
            from open in UnderlineToken
            from children in Parse.Ref(() => AnyNode).AtLeastOnce()
            from close in UnderlineToken
            select new FormattedContainerNode(TextFormatting.Underline, children.ToArray());

        private static readonly Parser<Node> StrikethroughFormattedContainerNode =
            from open in StrikethroughToken
            from children in Parse.Ref(() => AnyNode).AtLeastOnce()
            from close in StrikethroughToken
            select new FormattedContainerNode(TextFormatting.Strikethrough, children.ToArray());

        private static readonly Parser<Node> AnyFormattedContainerNode =
            BoldFormattedContainerNode
                .Or(UnderlineFormattedContainerNode)
                .Or(ItalicFormattedContainerNode)
                .Or(ItalicAltFormattedContainerNode)
                .Or(StrikethroughFormattedContainerNode);

        private static readonly Parser<Node> TextNode =
            Parse.AnyChar.Except(AnyFormattedContainerNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        private static readonly Parser<Node> AnyNode = AnyFormattedContainerNode.Or(TextNode);

        public IReadOnlyList<Node> Process(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}