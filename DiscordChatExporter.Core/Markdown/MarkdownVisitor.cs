using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Markdown.Ast;

namespace DiscordChatExporter.Core.Markdown
{
    internal abstract class MarkdownVisitor
    {
        protected virtual MarkdownNode VisitText(TextNode text) => text;

        protected virtual MarkdownNode VisitFormatted(FormattedNode formatted)
        {
            Visit(formatted.Children);
            return formatted;
        }

        protected virtual MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock) => inlineCodeBlock;

        protected virtual MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock) => multiLineCodeBlock;

        protected virtual MarkdownNode VisitLink(LinkNode link) => link;

        protected virtual MarkdownNode VisitEmoji(EmojiNode emoji) => emoji;

        protected virtual MarkdownNode VisitMention(MentionNode mention) => mention;

        public MarkdownNode Visit(MarkdownNode node) => node switch
        {
            TextNode text => VisitText(text),
            FormattedNode formatted => VisitFormatted(formatted),
            InlineCodeBlockNode inlineCodeBlock => VisitInlineCodeBlock(inlineCodeBlock),
            MultiLineCodeBlockNode multiLineCodeBlock => VisitMultiLineCodeBlock(multiLineCodeBlock),
            LinkNode link => VisitLink(link),
            EmojiNode emoji => VisitEmoji(emoji),
            MentionNode mention => VisitMention(mention),
            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };

        public void Visit(IEnumerable<MarkdownNode> nodes)
        {
            foreach (var node in nodes)
                Visit(node);
        }
    }
}