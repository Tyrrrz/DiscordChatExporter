using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal abstract class MarkdownVisitor
{
    protected virtual ValueTask<MarkdownNode> VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default) =>
        new(text);

    protected virtual async ValueTask<MarkdownNode> VisitFormattingAsync(
        FormattingNode formatting,
        CancellationToken cancellationToken = default)
    {
        await VisitAsync(formatting.Children, cancellationToken);
        return formatting;
    }

    protected virtual ValueTask<MarkdownNode> VisitInlineCodeBlockAsync(
        InlineCodeBlockNode inlineCodeBlock,
        CancellationToken cancellationToken = default) =>
        new(inlineCodeBlock);

    protected virtual ValueTask<MarkdownNode> VisitMultiLineCodeBlockAsync(
        MultiLineCodeBlockNode multiLineCodeBlock,
        CancellationToken cancellationToken = default) =>
        new(multiLineCodeBlock);

    protected virtual async ValueTask<MarkdownNode> VisitLinkAsync(
        LinkNode link,
        CancellationToken cancellationToken = default)
    {
        await VisitAsync(link.Children, cancellationToken);
        return link;
    }

    protected virtual ValueTask<MarkdownNode> VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default) =>
        new(emoji);

    protected virtual ValueTask<MarkdownNode> VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default) =>
        new(mention);

    protected virtual ValueTask<MarkdownNode> VisitUnixTimestampAsync(
        UnixTimestampNode timestamp,
        CancellationToken cancellationToken = default) =>
        new(timestamp);

    public async ValueTask<MarkdownNode> VisitAsync(
        MarkdownNode node,
        CancellationToken cancellationToken = default) => node switch
        {
            TextNode text =>
                await VisitTextAsync(text, cancellationToken),

            FormattingNode formatting =>
                await VisitFormattingAsync(formatting, cancellationToken),

            InlineCodeBlockNode inlineCodeBlock =>
                await VisitInlineCodeBlockAsync(inlineCodeBlock, cancellationToken),

            MultiLineCodeBlockNode multiLineCodeBlock =>
                await VisitMultiLineCodeBlockAsync(multiLineCodeBlock, cancellationToken),

            LinkNode link =>
                await VisitLinkAsync(link, cancellationToken),

            EmojiNode emoji =>
                await VisitEmojiAsync(emoji, cancellationToken),

            MentionNode mention =>
                await VisitMentionAsync(mention, cancellationToken),

            UnixTimestampNode timestamp =>
                await VisitUnixTimestampAsync(timestamp, cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };

    public async ValueTask VisitAsync(
        IEnumerable<MarkdownNode> nodes,
        CancellationToken cancellationToken = default)
    {
        foreach (var node in nodes)
            await VisitAsync(node, cancellationToken);
    }
}