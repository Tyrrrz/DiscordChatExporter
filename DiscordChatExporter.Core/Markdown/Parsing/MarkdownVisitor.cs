using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal abstract class MarkdownVisitor
{
    protected virtual MarkdownNode VisitText(TextNode text) =>
        text;

    protected virtual MarkdownNode VisitFormatting(FormattingNode formatting)
    {
        Visit(formatting.Children);
        return formatting;
    }

    protected virtual MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock) =>
        inlineCodeBlock;

    protected virtual MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock) =>
        multiLineCodeBlock;

    protected virtual MarkdownNode VisitLink(LinkNode link)
    {
        Visit(link.Children);
        return link;
    }

    protected virtual MarkdownNode VisitEmoji(EmojiNode emoji) =>
        emoji;

    protected virtual MarkdownNode VisitMention(MentionNode mention) =>
        mention;

    protected virtual MarkdownNode VisitUnixTimestamp(UnixTimestampNode timestamp) =>
        timestamp;

    public MarkdownNode Visit(MarkdownNode node) => node switch
    {
        TextNode text => VisitText(text),
        FormattingNode formatting => VisitFormatting(formatting),
        InlineCodeBlockNode inlineCodeBlock => VisitInlineCodeBlock(inlineCodeBlock),
        MultiLineCodeBlockNode multiLineCodeBlock => VisitMultiLineCodeBlock(multiLineCodeBlock),
        LinkNode link => VisitLink(link),
        EmojiNode emoji => VisitEmoji(emoji),
        MentionNode mention => VisitMention(mention),
        UnixTimestampNode timestamp => VisitUnixTimestamp(timestamp),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    public void Visit(IEnumerable<MarkdownNode> nodes)
    {
        foreach (var node in nodes)
            Visit(node);
    }
}