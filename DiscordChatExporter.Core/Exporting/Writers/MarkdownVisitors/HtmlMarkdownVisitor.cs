using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

    protected override MarkdownNode VisitText(TextNode text)
    {
        _buffer.Append(HtmlEncode(text.Text));
        return base.VisitText(text);
    }

    protected override MarkdownNode VisitFormatting(FormattingNode formatting)
    {
        var (tagOpen, tagClose) = formatting.Kind switch
        {
            FormattingKind.Bold => ("<strong>", "</strong>"),
            FormattingKind.Italic => ("<em>", "</em>"),
            FormattingKind.Underline => ("<u>", "</u>"),
            FormattingKind.Strikethrough => ("<s>", "</s>"),
            FormattingKind.Spoiler => (
                "<span class=\"spoiler-text spoiler-text--hidden\" onclick=\"showSpoiler(event, this)\">", "</span>"),
            FormattingKind.Quote => (
                "<div class=\"quote\"><div class=\"quote-border\"></div><div class=\"quote-content\">", "</div></div>"),
            _ => throw new ArgumentOutOfRangeException(nameof(formatting.Kind))
        };

        _buffer.Append(tagOpen);
        var result = base.VisitFormatting(formatting);
        _buffer.Append(tagClose);

        return result;
    }

    protected override MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock)
    {
        _buffer
            .Append("<span class=\"pre pre--inline\">")
            .Append(HtmlEncode(inlineCodeBlock.Code))
            .Append("</span>");

        return base.VisitInlineCodeBlock(inlineCodeBlock);
    }

    protected override MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock)
    {
        var highlightCssClass = !string.IsNullOrWhiteSpace(multiLineCodeBlock.Language)
            ? $"language-{multiLineCodeBlock.Language}"
            : "nohighlight";

        _buffer
            .Append($"<div class=\"pre pre--multiline {highlightCssClass}\">")
            .Append(HtmlEncode(multiLineCodeBlock.Code))
            .Append("</div>");

        return base.VisitMultiLineCodeBlock(multiLineCodeBlock);
    }

    protected override MarkdownNode VisitLink(LinkNode link)
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

        var result = base.VisitLink(link);
        _buffer.Append("</a>");

        return result;
    }

    protected override MarkdownNode VisitEmoji(EmojiNode emoji)
    {
        var emojiImageUrl = Emoji.GetImageUrl(emoji.Id, emoji.Name, emoji.IsAnimated);
        var jumboClass = _isJumbo ? "emoji--large" : "";

        _buffer
            .Append($"<img loading=\"lazy\" class=\"emoji {jumboClass}\" alt=\"{emoji.Name}\" title=\"{emoji.Code}\" src=\"{emojiImageUrl}\">");

        return base.VisitEmoji(emoji);
    }

    protected override MarkdownNode VisitMention(MentionNode mention)
    {
        if (mention.Kind == MentionKind.Everyone)
        {
            _buffer
                .Append("<span class=\"mention\">")
                .Append("@everyone")
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Here)
        {
            _buffer
                .Append("<span class=\"mention\">")
                .Append("@here")
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.User)
        {
            var member = mention.TargetId?.Pipe(_context.TryGetMember);
            var fullName = member?.User.FullName ?? "Unknown";
            var nick = member?.Nick ?? "Unknown";

            _buffer
                .Append($"<span class=\"mention\" title=\"{HtmlEncode(fullName)}\">")
                .Append('@').Append(HtmlEncode(nick))
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(_context.TryGetChannel);
            var symbol = channel?.IsVoiceChannel == true ? "🔊" : "#";
            var name = channel?.Name ?? "deleted-channel";

            _buffer
                .Append("<span class=\"mention\">")
                .Append(symbol).Append(HtmlEncode(name))
                .Append("</span>");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(_context.TryGetRole);
            var name = role?.Name ?? "deleted-role";
            var color = role?.Color;

            var style = color is not null
                ? $"color: rgb({color?.R}, {color?.G}, {color?.B}); background-color: rgba({color?.R}, {color?.G}, {color?.B}, 0.1);"
                : "";

            _buffer
                .Append($"<span class=\"mention\" style=\"{style}\">")
                .Append('@').Append(HtmlEncode(name))
                .Append("</span>");
        }

        return base.VisitMention(mention);
    }

    protected override MarkdownNode VisitUnixTimestamp(UnixTimestampNode timestamp)
    {
        var dateString = timestamp.Date is not null
            ? _context.FormatDate(timestamp.Date.Value)
            : "Invalid date";

        // Timestamp tooltips always use full date regardless of the configured format
        var longDateString = timestamp.Date is not null
            ? timestamp.Date.Value.ToLocalString("dddd, MMMM d, yyyy h:mm tt")
            : "Invalid date";

        _buffer
            .Append($"<span class=\"timestamp\" title=\"{HtmlEncode(longDateString)}\">")
            .Append(HtmlEncode(dateString))
            .Append("</span>");

        return base.VisitUnixTimestamp(timestamp);
    }
}

internal partial class HtmlMarkdownVisitor
{
    private static string HtmlEncode(string text) => WebUtility.HtmlEncode(text);

    public static string Format(ExportContext context, string markdown, bool isJumboAllowed = true)
    {
        var nodes = MarkdownParser.Parse(markdown);

        var isJumbo =
            isJumboAllowed &&
            nodes.All(n => n is EmojiNode || n is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Text));

        var buffer = new StringBuilder();

        new HtmlMarkdownVisitor(context, buffer, isJumbo).Visit(nodes);

        return buffer.ToString();
    }
}