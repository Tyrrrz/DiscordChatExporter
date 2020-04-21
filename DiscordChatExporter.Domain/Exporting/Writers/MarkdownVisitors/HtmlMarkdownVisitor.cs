using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Markdown;
using DiscordChatExporter.Domain.Markdown.Ast;

namespace DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors
{
    internal partial class HtmlMarkdownVisitor : MarkdownVisitor
    {
        private readonly RenderContext _context;
        private readonly StringBuilder _buffer;
        private readonly bool _isJumbo;

        public HtmlMarkdownVisitor(RenderContext context, StringBuilder buffer, bool isJumbo)
        {
            _context = context;
            _buffer = buffer;
            _isJumbo = isJumbo;
        }

        public override MarkdownNode VisitText(TextNode text)
        {
            _buffer.Append(HtmlEncode(text.Text));
            return base.VisitText(text);
        }

        public override MarkdownNode VisitFormatted(FormattedNode formatted)
        {
            var (tagOpen, tagClose) = formatted.Formatting switch
            {
                TextFormatting.Bold => ("<strong>", "</strong>"),
                TextFormatting.Italic => ("<em>", "</em>"),
                TextFormatting.Underline => ("<u>", "</u>"),
                TextFormatting.Strikethrough => ("<s>", "</s>"),
                TextFormatting.Spoiler => (
                    "<span class=\"spoiler spoiler--hidden\" onclick=\"showSpoiler(event, this)\"><span class=\"spoiler-text\">", "</span>"),
                TextFormatting.Quote => ("<div class=\"quote\">", "</div>"),
                _ => throw new ArgumentOutOfRangeException(nameof(formatted.Formatting))
            };

            _buffer.Append(tagOpen);
            var result = base.VisitFormatted(formatted);
            _buffer.Append(tagClose);

            return result;
        }

        public override MarkdownNode VisitInlineCodeBlock(InlineCodeBlockNode inlineCodeBlock)
        {
            _buffer
                .Append("<span class=\"pre pre--inline\">")
                .Append(HtmlEncode(inlineCodeBlock.Code))
                .Append("</span>");

            return base.VisitInlineCodeBlock(inlineCodeBlock);
        }

        public override MarkdownNode VisitMultiLineCodeBlock(MultiLineCodeBlockNode multiLineCodeBlock)
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

        public override MarkdownNode VisitMention(MentionNode mention)
        {
            if (mention.Type == MentionType.Meta)
            {
                _buffer
                    .Append("<span class=\"mention\">")
                    .Append("@").Append(HtmlEncode(mention.Id))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.User)
            {
                var user = _context.MentionableUsers.FirstOrDefault(u => u.Id == mention.Id) ??
                           User.CreateUnknownUser(mention.Id);

                var nick = Guild.GetUserNick(_context.Guild, user);

                _buffer
                    .Append($"<span class=\"mention\" title=\"{HtmlEncode(user.FullName)}\">")
                    .Append("@").Append(HtmlEncode(nick))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.Channel)
            {
                var channel = _context.MentionableChannels.FirstOrDefault(c => c.Id == mention.Id) ??
                              Channel.CreateDeletedChannel(mention.Id);

                _buffer
                    .Append("<span class=\"mention\">")
                    .Append("#").Append(HtmlEncode(channel.Name))
                    .Append("</span>");
            }
            else if (mention.Type == MentionType.Role)
            {
                var role = _context.MentionableRoles.FirstOrDefault(r => r.Id == mention.Id) ??
                           Role.CreateDeletedRole(mention.Id);

                var style = role.Color != null
                    ? $"color: {role.Color.Value.ToHexString()}; background-color: rgba({role.Color.Value.ToRgbString()}, 0.1);"
                    : "";

                _buffer
                    .Append($"<span class=\"mention\" style=\"{style}>\"")
                    .Append("@").Append(HtmlEncode(role.Name))
                    .Append("</span>");
            }

            return base.VisitMention(mention);
        }

        public override MarkdownNode VisitEmoji(EmojiNode emoji)
        {
            var emojiImageUrl = Emoji.GetImageUrl(emoji.Id, emoji.Name, emoji.IsAnimated);
            var jumboClass = _isJumbo ? "emoji--large" : "";

            _buffer
                .Append($"<img class=\"emoji {jumboClass}\" alt=\"{emoji.Name}\" title=\"{emoji.Name}\" src=\"{emojiImageUrl}\" />");

            return base.VisitEmoji(emoji);
        }

        public override MarkdownNode VisitLink(LinkNode link)
        {
            // Extract message ID if the link points to a Discord message
            var linkedMessageId = Regex.Match(link.Url, "^https?://discordapp.com/channels/.*?/(\\d+)/?$").Groups[1].Value;

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

        public static string Format(RenderContext context, string markdown)
        {
            var nodes = MarkdownParser.Parse(markdown);
            var isJumbo = nodes.All(n => n is EmojiNode || n is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Text));

            var buffer = new StringBuilder();

            new HtmlMarkdownVisitor(context, buffer, isJumbo).Visit(nodes);

            return buffer.ToString();
        }
    }
}