using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private static Parser<string> NonGreedyText<T, V>(Parser<T> otherParser, Parser<V> otherParser2) =>
            Parse.AnyChar.Except(otherParser).Except(otherParser2).AtLeastOnce().Text();

        private static Parser<Node> FormattedContainerNode(Parser<string> token, TextFormatting formatting) =>
            from open in token
            from children in Parse.Ref(() => AnyFormattedContainerNode).Or(NonGreedyText(AnyFormattedContainerNode, token).Select(s => new TextNode(s))).Except(token).AtLeastOnce()
            from close in token
            select new FormattedContainerNode(formatting, children.ToArray());

        private static readonly Parser<Node> BoldFormattedContainerNode =
            FormattedContainerNode(BoldToken, TextFormatting.Bold);

        private static readonly Parser<Node> ItalicFormattedContainerNode =
            FormattedContainerNode(ItalicToken, TextFormatting.Italic);

        private static readonly Parser<Node> ItalicAltFormattedContainerNode =
            FormattedContainerNode(ItalicAltToken, TextFormatting.Italic);

        private static readonly Parser<Node> UnderlineFormattedContainerNode =
            FormattedContainerNode(UnderlineToken, TextFormatting.Underline);

        private static readonly Parser<Node> StrikethroughFormattedContainerNode =
            FormattedContainerNode(StrikethroughToken, TextFormatting.Strikethrough);

        private static readonly Parser<Node> AnyFormattedContainerNode =
            BoldFormattedContainerNode
                .Or(UnderlineFormattedContainerNode)
                .Or(ItalicFormattedContainerNode)
                .Or(ItalicAltFormattedContainerNode)
                .Or(StrikethroughFormattedContainerNode);

        private static readonly Parser<Node> TextNode =
            Parse.AnyChar.Except(AnyFormattedContainerNode).AtLeastOnce().Text().Select(s => new TextNode(s));

        private static readonly Parser<Node> AnyNode = AnyFormattedContainerNode.Or(TextNode);

        private static IEnumerable<Node> MergeNodes(IEnumerable<Node> nodes)
        {
            var buffer = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node is TextNode textNode)
                {
                    buffer.Append(textNode.Text);
                }
                else
                {
                    if (buffer.Length > 0)
                    {
                        yield return new TextNode(buffer.ToString());
                        buffer.Clear();
                    }

                    yield return node;
                }
            }

            if (buffer.Length > 0)
            {
                yield return new TextNode(buffer.ToString());
                buffer.Clear();
            }
        }

        public IReadOnlyList<Node> Process(string input) => AnyNode.Many().Parse(input).ToArray();
    }
}