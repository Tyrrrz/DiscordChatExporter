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

namespace DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;

internal partial class HtmlMarkdownVisitor : MarkdownVisitor
{
    private readonly ExportContext _context;
    private readonly StringBuilder _buffer;
    private readonly bool _isJumbo;

    public HtmlMarkdownVisitor(ExportContext context, StringBuilder buffer, bool isJumbo)
    {
        _context = context;
        _buffer = buffer;
        _isJumbo = isJumbo;
    }

    protected override async ValueTask<MarkdownNode> VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default)
    {
        _buffer.Append(HtmlEncode(text.Text));
        return await base.VisitTextAsync(text, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitFormattingAsync(
        FormattingNode formatting,
        CancellationToken cancellationToken = default)
    {
        var (openingTag, closingTag) = formatting.Kind switch
        {
            FormattingKind.Bold => (
                "<strong>",
                "</strong>"
            ),

            FormattingKind.Italic => (
                "<em>",
                "</em>"
            ),

            FormattingKind.Underline => (
                "<u>",
                "</u>"
            ),

            FormattingKind.Strikethrough => (
                "<s>",
                "</s>"
            ),

            FormattingKind.Spoiler => (
                "<span class=\"chatlog__markdown-spoiler chatlog__markdown-spoiler--hidden\" onclick=\"showSpoiler(event, this)\">",
                "</span>"
            ),

            FormattingKind.Quote => (
                "<div class=\"chatlog__markdown-quote\"><div class=\"chatlog__markdown-quote-border\"></div><div class=\"chatlog__markdown-quote-content\">",
                "</div></div>"
            ),

            _ => throw new InvalidOperationException($"Unknown formatting kind '{formatting.Kind}'.")
        };

        _buffer.Append(openingTag);
        var result = await base.VisitFormattingAsync(formatting, cancellationToken);
        _buffer.Append(closingTag);

        return result;
    }

    protected override async ValueTask<MarkdownNode> VisitInlineCodeBlockAsync(
        InlineCodeBlockNode inlineCodeBlock,
        CancellationToken cancellationToken = default)
    {
        _buffer
            .Append("<code class=\"chatlog__markdown-pre chatlog__markdown-pre--inline\">")
            .Append(HtmlEncode(inlineCodeBlock.Code))
            .Append("</code>");

        return await base.VisitInlineCodeBlockAsync(inlineCodeBlock, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitMultiLineCodeBlockAsync(
        MultiLineCodeBlockNode multiLineCodeBlock,
        CancellationToken cancellationToken = default)
    {
        var highlightCssClass = !string.IsNullOrWhiteSpace(multiLineCodeBlock.Language)
            ? $"language-{multiLineCodeBlock.Language}"
            : "nohighlight";

        _buffer
            .Append($"<code class=\"chatlog__markdown-pre chatlog__markdown-pre--multiline {highlightCssClass}\">")
            .Append(HtmlEncode(multiLineCodeBlock.Code))
            .Append("</code>");

        return await base.VisitMultiLineCodeBlockAsync(multiLineCodeBlock, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitLinkAsync(
        LinkNode link,
        CancellationToken cancellationToken = default)
    {
        // Try to extract message ID if the link refers to a Discord message
        var linkedMessageId = Regex.Match(
            link.Url,
            "^https?://(?:discord|discordapp).com/channels/.*?/(\\d+)/?$"
        ).Groups[1].Value;

        _buffer.Append(
            !string.IsNullOrWhiteSpace(linkedMessageId)
                ? $"<a href=\"{HtmlEncode(link.Url)}\" onclick=\"scrollToMessage(event, '{linkedMessageId}')\">"
                : $"<a href=\"{HtmlEncode(link.Url)}\">"
        );

        var result = await base.VisitLinkAsync(link, cancellationToken);
        _buffer.Append("</a>");

        return result;
    }

    protected override async ValueTask<MarkdownNode> VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default)
    {
        var emojiImageUrl = Emoji.GetImageUrl(emoji.Id, emoji.Name, emoji.IsAnimated);
        var jumboClass = _isJumbo ? "chatlog__emoji--large" : "";

        _buffer.Append(
            $"<img " +
            $"loading=\"lazy\" " +
            $"class=\"chatlog__emoji {jumboClass}\" " +
            $"alt=\"{emoji.Name}\" " +
            $"title=\"{emoji.Code}\" " +
            $"src=\"{await _context.ResolveAssetUrlAsync(emojiImageUrl, cancellationToken)}\"" +
            $">"
        );

        return await base.VisitEmojiAsync(emoji, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default)
    {
        if (mention.Kind == MentionKind.Everyone)
        {
            _buffer
                .Append("<span class=\"chatlog__markdown-mention\">")
                .Append("@everyone")
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Here)
        {
            _buffer
                .Append("<span class=\"chatlog__markdown-mention\">")
                .Append("@here")
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.User)
        {
            var member = mention.TargetId?.Pipe(_context.TryGetMember);
            var fullName = member?.User.FullName ?? "Unknown";
            var nick = member?.Nick ?? "Unknown";

            _buffer
                .Append($"<span class=\"chatlog__markdown-mention\" title=\"{HtmlEncode(fullName)}\">")
                .Append('@').Append(HtmlEncode(nick))
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(_context.TryGetChannel);
            var symbol = channel?.SupportsVoice == true ? "🔊" : "#";
            var name = channel?.Name ?? "deleted-channel";

            _buffer
                .Append("<span class=\"chatlog__markdown-mention\">")
                .Append(symbol).Append(HtmlEncode(name))
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(_context.TryGetRole);
            var name = role?.Name ?? "deleted-role";
            var color = role?.Color;

            var style = color is not null
                ? $"color: rgb({color.Value.R}, {color.Value.G}, {color.Value.B}); " +
                  $"background-color: rgba({color.Value.R}, {color.Value.G}, {color.Value.B}, 0.1);"
                : "";

            _buffer
                .Append($"<span class=\"chatlog__markdown-mention\" style=\"{style}\">")
                .Append('@').Append(HtmlEncode(name))
                .Append("</span>");
        }

        return await base.VisitMentionAsync(mention, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitUnixTimestampAsync(
        UnixTimestampNode timestamp,
        CancellationToken cancellationToken = default)
    {
        var dateString = timestamp.Date is not null
            ? _context.FormatDate(timestamp.Date.Value)
            : "Invalid date";

        // Timestamp tooltips always use full date regardless of the configured format
        var longDateString = timestamp.Date is not null
            ? timestamp.Date.Value.ToLocalString("dddd, MMMM d, yyyy h:mm tt")
            : "Invalid date";

        _buffer
            .Append($"<span class=\"chatlog__markdown-timestamp\" title=\"{HtmlEncode(longDateString)}\">")
            .Append(HtmlEncode(dateString))
            .Append("</span>");

        return await base.VisitUnixTimestampAsync(timestamp, cancellationToken);
    }
}

internal partial class HtmlMarkdownVisitor
{
    private static string HtmlEncode(string text) => WebUtility.HtmlEncode(text);

    public static async ValueTask<string> FormatAsync(
        ExportContext context,
        string markdown,
        bool isJumboAllowed = true,
        CancellationToken cancellationToken = default)
    {
        var nodes = MarkdownParser.Parse(markdown);

        var isJumbo =
            isJumboAllowed &&
            nodes.All(n => n is EmojiNode || n is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Text));

        var buffer = new StringBuilder();

        await new HtmlMarkdownVisitor(context, buffer, isJumbo)
            .VisitAsync(nodes, cancellationToken);

        return buffer.ToString();
    }
}