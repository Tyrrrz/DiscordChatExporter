using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Ast;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors
{
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

        protected override MarkdownNode VisitFormatted(FormattedNode formatted)
        {
            var (tagOpen, tagClose) = formatted.Formatting switch
            {
                TextFormatting.Bold => ("<strong>", "</strong>"),
                TextFormatting.Italic => ("<em>", "</em>"),
                TextFormatting.Underline => ("<u>", "</u>"),
                TextFormatting.Strikethrough => ("<s>", "</s>"),
                TextFormatting.Spoiler => (
                    "<span class=\"spoiler spoiler--hidden\" onclick=\"showSpoiler(event, this)\"><span class=\"spoiler-text\">", "</span></span>"),
                TextFormatting.Quote => ("<div class=\"quote\">", "</div>"),
                _ => throw new ArgumentOutOfRangeException(nameof(formatted.Formatting))
            };

            _buffer.Append(tagOpen);
            var result = base.VisitFormatted(formatted);
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

        protected override MarkdownNode VisitMention(MentionNode mention)
        {
            var mentionId = Snowflake.TryParse(mention.Id);
            if (mention.Type == MentionType.Meta)
            {
                _buffer
                    .Append("<span class=\"mention\">")
                    .Append("@").Append(HtmlEncode(mention.Id))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.User)
            {
                var member = mentionId?.Pipe(_context.TryGetMember);
                var fullName = member?.User.FullName ?? "Unknown";
                var nick = member?.Nick ?? "Unknown";

                _buffer
                    .Append($"<span class=\"mention\" title=\"{HtmlEncode(fullName)}\">")
                    .Append("@").Append(HtmlEncode(nick))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.Channel)
            {
                var channel = mentionId?.Pipe(_context.TryGetChannel);
                var name = channel?.Name ?? "deleted-channel";

                _buffer
                    .Append("<span class=\"mention\">")
                    .Append("#").Append(HtmlEncode(name))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.Role)
            {
                var role = mentionId?.Pipe(_context.TryGetRole);
                var name = role?.Name ?? "deleted-role";
                var color = role?.Color;

                var style = color is not null
                    ? $"color: rgb({color?.R}, {color?.G}, {color?.B}); background-color: rgba({color?.R}, {color?.G}, {color?.B}, 0.1);"
                    : "";

                _buffer
                    .Append($"<span class=\"mention\" style=\"{style}\">")
                    .Append("@").Append(HtmlEncode(name))
                    .Append("</span>");
            }

            return base.VisitMention(mention);
        }

        protected override MarkdownNode VisitEmoji(EmojiNode emoji)
        {
            var emojiImageUrl = Emoji.GetImageUrl(emoji.Id, emoji.Name, emoji.IsAnimated);
            var jumboClass = _isJumbo ? "emoji--large" : "";

            _buffer
                .Append($"<img loading=\"lazy\" class=\"emoji {jumboClass}\" alt=\"{emoji.Name}\" title=\"{emoji.Code}\" src=\"{emojiImageUrl}\">");

            return base.VisitEmoji(emoji);
        }

        protected override MarkdownNode VisitLink(LinkNode link)
        {
            // Extract message ID if the link points to a Discord message
            var linkedMessageId = Regex.Match(link.Url, "^https?://(?:discord|discordapp).com/channels/.*?/(\\d+)/?$").Groups[1].Value;

            if (!string.IsNullOrWhiteSpace(linkedMessageId))
            {
                _buffer
                    .Append($"<a href=\"{Uri.EscapeUriString(link.Url)}\" onclick=\"scrollToMessage(event, '{linkedMessageId}')\">")
                    .Append(HtmlEncode(link.Title))
                    .Append("</a>");
            }
            else
            {
                _buffer
                    .Append($"<a href=\"{Uri.EscapeUriString(link.Url)}\">")
                    .Append(HtmlEncode(link.Title))
                    .Append("</a>");
            }

            return base.VisitLink(link);
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
}
