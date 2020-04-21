using System;
using System.Collections.Generic;
using DiscordChatExporter.Domain.Markdown.Ast;

namespace DiscordChatExporter.Domain.Markdown
{
    internal abstract class MarkdownVisitor
    {
        public virtual MarkdownNode VisitText(TextNode text) => text;

        public virtual MarkdownNode VisitFormatted(FormattedNode formatted)
        {
            Visit(formatted.Children);
            return formatted;
        }

        public virtual MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock) => inlineCodeBlock;

        public virtual MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock) => multiLineCodeBlock;

        public virtual MarkdownNode VisitLink(LinkNode link) => link;

        public virtual MarkdownNode VisitEmoji(EmojiNode emoji) => emoji;

        public virtual MarkdownNode VisitMention(MentionNode mention) => mention;

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