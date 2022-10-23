using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal abstract class MarkdownVisitor
{
    protected virtual MarkdownNode VisitText(TextNode text) =>
        text;

    protected virtual async ValueTask<MarkdownNode> VisitFormatting(FormattingNode formatting)
    {
        await Visit(formatting.Children);
        return formatting;
    }

    protected virtual MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock) =>
        inlineCodeBlock;

    protected virtual MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock) =>
        multiLineCodeBlock;

    protected virtual async ValueTask<MarkdownNode> VisitLink(LinkNode link)
    {
        await Visit(link.Children);
        return link;
    }

    protected virtual ValueTask<MarkdownNode> VisitEmoji(EmojiNode emoji) => new(emoji);

    protected virtual MarkdownNode VisitMention(MentionNode mention) =>
        mention;

    protected virtual MarkdownNode VisitUnixTimestamp(UnixTimestampNode timestamp) =>
        timestamp;

    public async ValueTask<MarkdownNode> Visit(MarkdownNode node) => node switch
    {
        TextNode text => VisitText(text),
        FormattingNode formatting => await VisitFormatting(formatting),
        InlineCodeBlockNode inlineCodeBlock => VisitInlineCodeBlock(inlineCodeBlock),
        MultiLineCodeBlockNode multiLineCodeBlock => VisitMultiLineCodeBlock(multiLineCodeBlock),
        LinkNode link => await VisitLink(link),
        EmojiNode emoji => await VisitEmoji(emoji),
        MentionNode mention => VisitMention(mention),
        UnixTimestampNode timestamp => VisitUnixTimestamp(timestamp),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    public async ValueTask Visit(IEnumerable<MarkdownNode> nodes)
    {
        foreach (var node in nodes)
            await Visit(node);
    }
}