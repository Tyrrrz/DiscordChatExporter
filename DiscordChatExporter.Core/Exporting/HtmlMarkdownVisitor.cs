using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Parsing;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class HtmlMarkdownVisitor(
    ExportContext context,
    StringBuilder buffer,
    bool isJumbo
) : MarkdownVisitor
{
    protected override ValueTask VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(HtmlEncode(text.Text));
        return default;
    }

    protected override async ValueTask VisitFormattingAsync(
        FormattingNode formatting,
        CancellationToken cancellationToken = default
    )
    {
        var (openingTag, closingTag) = formatting.Kind switch
        {
            FormattingKind.Bold
                => (
                    // lang=html
                    "<strong>",
                    // lang=html
                    "</strong>"
                ),

            FormattingKind.Italic
                => (
                    // lang=html
                    "<em>",
                    // lang=html
                    "</em>"
                ),

            FormattingKind.Underline
                => (
                    // lang=html
                    "<u>",
                    // lang=html
                    "</u>"
                ),

            FormattingKind.Strikethrough
                => (
                    // lang=html
                    "<s>",
                    // lang=html
                    "</s>"
                ),

            FormattingKind.Spoiler
                => (
                    // lang=html
                    """<span class="chatlog__markdown-spoiler chatlog__markdown-spoiler--hidden" onclick="showSpoiler(event, this)">""",
                    // lang=html
                    """</span>"""
                ),

            FormattingKind.Quote
                => (
                    // lang=html
                    """<div class="chatlog__markdown-quote"><div class="chatlog__markdown-quote-border"></div><div class="chatlog__markdown-quote-content">""",
                    // lang=html
                    """</div></div>"""
                ),

            _
                => throw new InvalidOperationException(
                    $"Unknown formatting kind '{formatting.Kind}'."
                )
        };

        buffer.Append(openingTag);
        await VisitAsync(formatting.Children, cancellationToken);
        buffer.Append(closingTag);
    }

    protected override async ValueTask VisitHeadingAsync(
        HeadingNode heading,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(
            // lang=html
            $"<h{heading.Level}>"
        );

        await VisitAsync(heading.Children, cancellationToken);

        buffer.Append(
            // lang=html
            $"</h{heading.Level}>"
        );
    }

    protected override async ValueTask VisitListAsync(
        ListNode list,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(
            // lang=html
            "<ul>"
        );

        await VisitAsync(list.Items, cancellationToken);

        buffer.Append(
            // lang=html
            "</ul>"
        );
    }

    protected override async ValueTask VisitListItemAsync(
        ListItemNode listItem,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(
            // lang=html
            "<li>"
        );

        await VisitAsync(listItem.Children, cancellationToken);

        buffer.Append(
            // lang=html
            "</li>"
        );
    }

    protected override ValueTask VisitInlineCodeBlockAsync(
        InlineCodeBlockNode inlineCodeBlock,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(
            // lang=html
            $"""
            <code class="chatlog__markdown-pre chatlog__markdown-pre--inline">{HtmlEncode(
                inlineCodeBlock.Code
            )}</code>
            """
        );

        return default;
    }

    protected override ValueTask VisitMultiLineCodeBlockAsync(
        MultiLineCodeBlockNode multiLineCodeBlock,
        CancellationToken cancellationToken = default
    )
    {
        var highlightClass = !string.IsNullOrWhiteSpace(multiLineCodeBlock.Language)
            ? $"language-{multiLineCodeBlock.Language}"
            : "nohighlight";

        buffer.Append(
            // lang=html
            $"""
            <code class="chatlog__markdown-pre chatlog__markdown-pre--multiline {highlightClass}">{HtmlEncode(
                multiLineCodeBlock.Code
            )}</code>
            """
        );

        return default;
    }

    protected override async ValueTask VisitLinkAsync(
        LinkNode link,
        CancellationToken cancellationToken = default
    )
    {
        // Try to extract the message ID if the link points to a Discord message
        var linkedMessageId = Regex
            .Match(link.Url, @"^https?://(?:discord|discordapp)\.com/channels/.*?/(\d+)/?$")
            .Groups[1]
            .Value;

        buffer.Append(
            !string.IsNullOrWhiteSpace(linkedMessageId)
                // lang=html
                ? $"""<a href="{HtmlEncode(link.Url)}" onclick="scrollToMessage(event, '{linkedMessageId}')">"""
                // lang=html
                : $"""<a href="{HtmlEncode(link.Url)}">"""
        );

        await VisitAsync(link.Children, cancellationToken);

        buffer.Append(
            // lang=html
            "</a>"
        );
    }

    protected override async ValueTask VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default
    )
    {
        var emojiImageUrl = Emoji.GetImageUrl(emoji.Id, emoji.Name, emoji.IsAnimated);
        var jumboClass = isJumbo ? "chatlog__emoji--large" : "";

        buffer.Append(
            // lang=html
            $"""
            <img
                loading="lazy"
                class="chatlog__emoji {jumboClass}"
                alt="{emoji.Name}"
                title="{emoji.Code}"
                src="{await context.ResolveAssetUrlAsync(emojiImageUrl, cancellationToken)}">
            """
        );
    }

    protected override async ValueTask VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default
    )
    {
        if (mention.Kind == MentionKind.Everyone)
        {
            buffer.Append(
                // lang=html
                """
                <span class="chatlog__markdown-mention">@everyone</span>
                """
            );
        }
        else if (mention.Kind == MentionKind.Here)
        {
            buffer.Append(
                // lang=html
                """
                <span class="chatlog__markdown-mention">@here</span>
                """
            );
        }
        else if (mention.Kind == MentionKind.User)
        {
            // User mentions are not always included in the message object,
            // which means they need to be populated on demand.
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/304
            if (mention.TargetId is not null)
                await context.PopulateMemberAsync(mention.TargetId.Value, cancellationToken);

            var member = mention.TargetId?.Pipe(context.TryGetMember);
            var fullName = member?.User.FullName ?? "Unknown";
            var displayName = member?.DisplayName ?? member?.User.DisplayName ?? "Unknown";

            buffer.Append(
                // lang=html
                $"""
                <span class="chatlog__markdown-mention" title="{HtmlEncode(fullName)}">@{HtmlEncode(
                    displayName
                )}</span>
                """
            );
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(context.TryGetChannel);
            var symbol = channel?.IsVoice == true ? "🔊" : "#";
            var name = channel?.Name ?? "deleted-channel";

            buffer.Append(
                // lang=html
                $"""
                <span class="chatlog__markdown-mention">{symbol}{HtmlEncode(name)}</span>
                """
            );
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(context.TryGetRole);
            var name = role?.Name ?? "deleted-role";
            var color = role?.Color;

            var style = color is not null
                ? $"""
                    color: rgb({color.Value.R}, {color.Value.G}, {color
                        .Value
                        .B}); background-color: rgba({color.Value.R}, {color.Value.G}, {color
                        .Value
                        .B}, 0.1);
                    """
                : null;

            buffer.Append(
                // lang=html
                $"""
                <span class="chatlog__markdown-mention" style="{style}">@{HtmlEncode(name)}</span>
                """
            );
        }
    }

    protected override ValueTask VisitTimestampAsync(
        TimestampNode timestamp,
        CancellationToken cancellationToken = default
    )
    {
        var formatted = timestamp.Instant is not null
            ? context.FormatDate(timestamp.Instant.Value, timestamp.Format ?? "g")
            : "Invalid date";

        var formattedLong = timestamp.Instant is not null
            ? context.FormatDate(timestamp.Instant.Value, "f")
            : "";

        buffer.Append(
            // lang=html
            $"""
            <span class="chatlog__markdown-timestamp" title="{HtmlEncode(
                formattedLong
            )}">{HtmlEncode(formatted)}</span>
            """
        );

        return default;
    }
}

internal partial class HtmlMarkdownVisitor
{
    private static string HtmlEncode(string text) => WebUtility.HtmlEncode(text);

    public static async ValueTask<string> FormatAsync(
        ExportContext context,
        string markdown,
        bool isJumboAllowed = true,
        CancellationToken cancellationToken = default
    )
    {
        var nodes = MarkdownParser.Parse(markdown);

        var isJumbo =
            isJumboAllowed
            && nodes.All(n =>
                n is EmojiNode || n is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Text)
            );

        var buffer = new StringBuilder();
        await new HtmlMarkdownVisitor(context, buffer, isJumbo).VisitAsync(
            nodes,
            cancellationToken
        );

        return buffer.ToString();
    }
}
