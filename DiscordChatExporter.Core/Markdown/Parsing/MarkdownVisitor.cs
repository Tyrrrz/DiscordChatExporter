using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal abstract class MarkdownVisitor
{
    protected virtual ValueTask<MarkdownNode> VisitTextAsync(TextNode text) => 
        new(text);

    protected virtual async ValueTask<MarkdownNode> VisitFormattingAsync(FormattingNode formatting)
    {
        await VisitAsync(formatting.Children);
        return formatting;
    }

    protected virtual ValueTask<MarkdownNode> VisitInlineCodeBlockAsync(InlineCodeBlockNode inlineCodeBlock) => 
        new(inlineCodeBlock);

    protected virtual ValueTask<MarkdownNode> VisitMultiLineCodeBlockAsync(MultiLineCodeBlockNode multiLineCodeBlock) => 
        new(multiLineCodeBlock);

    protected virtual async ValueTask<MarkdownNode> VisitLinkAsync(LinkNode link)
    {
        await VisitAsync(link.Children);
        return link;
    }

    protected virtual ValueTask<MarkdownNode> VisitEmojiAsync(EmojiNode emoji) => 
        new(emoji);

    protected virtual ValueTask<MarkdownNode> VisitMentionAsync(MentionNode mention) => 
        new(mention);

    protected virtual ValueTask<MarkdownNode> VisitUnixTimestampAsync(UnixTimestampNode timestamp) => 
        new(timestamp);

    public async ValueTask<MarkdownNode> VisitAsync(MarkdownNode node) => node switch
    {
        TextNode text => await VisitTextAsync(text),
        FormattingNode formatting => await VisitFormattingAsync(formatting),
        InlineCodeBlockNode inlineCodeBlock => await VisitInlineCodeBlockAsync(inlineCodeBlock),
        MultiLineCodeBlockNode multiLineCodeBlock => await VisitMultiLineCodeBlockAsync(multiLineCodeBlock),
        LinkNode link => await VisitLinkAsync(link),
        EmojiNode emoji => await VisitEmojiAsync(emoji),
        MentionNode mention => await VisitMentionAsync(mention),
        UnixTimestampNode timestamp => await VisitUnixTimestampAsync(timestamp),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    public async ValueTask VisitAsync(IEnumerable<MarkdownNode> nodes)
    {
        foreach (var node in nodes)
            await VisitAsync(node);
    }
}