using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal abstract class MarkdownVisitor
{
    protected virtual ValueTask VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default
    ) => default;

    protected virtual async ValueTask VisitFormattingAsync(
        FormattingNode formatting,
        CancellationToken cancellationToken = default
    ) => await VisitAsync(formatting.Children, cancellationToken);

    protected virtual async ValueTask VisitHeadingAsync(
        HeadingNode heading,
        CancellationToken cancellationToken = default
    ) => await VisitAsync(heading.Children, cancellationToken);

    protected virtual async ValueTask VisitListAsync(
        ListNode list,
        CancellationToken cancellationToken = default
    ) => await VisitAsync(list.Items, cancellationToken);

    protected virtual async ValueTask VisitListItemAsync(
        ListItemNode listItem,
        CancellationToken cancellationToken = default
    ) => await VisitAsync(listItem.Children, cancellationToken);

    protected virtual ValueTask VisitInlineCodeBlockAsync(
        InlineCodeBlockNode inlineCodeBlock,
        CancellationToken cancellationToken = default
    ) => default;

    protected virtual ValueTask VisitMultiLineCodeBlockAsync(
        MultiLineCodeBlockNode multiLineCodeBlock,
        CancellationToken cancellationToken = default
    ) => default;

    protected virtual async ValueTask VisitLinkAsync(
        LinkNode link,
        CancellationToken cancellationToken = default
    ) => await VisitAsync(link.Children, cancellationToken);

    protected virtual ValueTask VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default
    ) => default;

    protected virtual ValueTask VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default
    ) => default;

    protected virtual ValueTask VisitTimestampAsync(
        TimestampNode timestamp,
        CancellationToken cancellationToken = default
    ) => default;

    public async ValueTask VisitAsync(
        MarkdownNode node,
        CancellationToken cancellationToken = default
    )
    {
        if (node is TextNode text)
        {
            await VisitTextAsync(text, cancellationToken);
            return;
        }

        if (node is FormattingNode formatting)
        {
            await VisitFormattingAsync(formatting, cancellationToken);
            return;
        }

        if (node is HeadingNode heading)
        {
            await VisitHeadingAsync(heading, cancellationToken);
            return;
        }

        if (node is ListNode list)
        {
            await VisitListAsync(list, cancellationToken);
            return;
        }

        if (node is ListItemNode listItem)
        {
            await VisitListItemAsync(listItem, cancellationToken);
            return;
        }

        if (node is InlineCodeBlockNode inlineCodeBlock)
        {
            await VisitInlineCodeBlockAsync(inlineCodeBlock, cancellationToken);
            return;
        }

        if (node is MultiLineCodeBlockNode multiLineCodeBlock)
        {
            await VisitMultiLineCodeBlockAsync(multiLineCodeBlock, cancellationToken);
            return;
        }

        if (node is LinkNode link)
        {
            await VisitLinkAsync(link, cancellationToken);
            return;
        }

        if (node is EmojiNode emoji)
        {
            await VisitEmojiAsync(emoji, cancellationToken);
            return;
        }

        if (node is MentionNode mention)
        {
            await VisitMentionAsync(mention, cancellationToken);
            return;
        }

        if (node is TimestampNode timestamp)
        {
            await VisitTimestampAsync(timestamp, cancellationToken);
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(node));
    }

    public async ValueTask VisitAsync(
        IEnumerable<MarkdownNode> nodes,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var node in nodes)
            await VisitAsync(node, cancellationToken);
    }
}
