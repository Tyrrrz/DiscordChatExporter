using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Nodes;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    public partial class HtmlChatLogRenderer : IChatLogRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _themeName;
        private readonly string _dateFormat;

        public HtmlChatLogRenderer(ChatLog chatLog, string themeName, string dateFormat)
        {
            _chatLog = chatLog;
            _themeName = themeName;
            _dateFormat = dateFormat;
        }

        private string HtmlEncode(string s) => WebUtility.HtmlEncode(s);

        private string FormatDate(DateTimeOffset date) =>
            date.ToLocalTime().ToString(_dateFormat, CultureInfo.InvariantCulture);

        private IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages) =>
            messages.GroupContiguous((buffer, message) =>
            {
                // Break group if the author changed
                if (buffer.Last().Author.Id != message.Author.Id)
                    return false;

                // Break group if last message was more than 7 minutes ago
                if ((message.Timestamp - buffer.Last().Timestamp).TotalMinutes > 7)
                    return false;

                return true;
            }).Select(g => new MessageGroup(g.First().Author, g.First().Timestamp, g));

        private string FormatMarkdown(Node node, bool isJumbo)
        {
            // Text node
            if (node is TextNode textNode)
            {
                // Return HTML-encoded text
                return HtmlEncode(textNode.Text);
            }

            // Formatted node
            if (node is FormattedNode formattedNode)
            {
                // Recursively get inner html
                var innerHtml = FormatMarkdown(formattedNode.Children, false);

                // Bold
                if (formattedNode.Formatting == TextFormatting.Bold)
                    return $"<strong>{innerHtml}</strong>";

                // Italic
                if (formattedNode.Formatting == TextFormatting.Italic)
                    return $"<em>{innerHtml}</em>";

                // Underline
                if (formattedNode.Formatting == TextFormatting.Underline)
                    return $"<u>{innerHtml}</u>";

                // Strikethrough
                if (formattedNode.Formatting == TextFormatting.Strikethrough)
                    return $"<s>{innerHtml}</s>";

                // Spoiler
                if (formattedNode.Formatting == TextFormatting.Spoiler)
                    return $"<span class=\"spoiler\">{innerHtml}</span>";
            }

            // Inline code block node
            if (node is InlineCodeBlockNode inlineCodeBlockNode)
            {
                return $"<span class=\"pre pre--inline\">{HtmlEncode(inlineCodeBlockNode.Code)}</span>";
            }

            // Multi-line code block node
            if (node is MultilineCodeBlockNode multilineCodeBlockNode)
            {
                // Set CSS class for syntax highlighting
                var highlightCssClass = !multilineCodeBlockNode.Language.IsNullOrWhiteSpace()
                    ? $"language-{multilineCodeBlockNode.Language}"
                    : "nohighlight";

                return $"<div class=\"pre pre--multiline {highlightCssClass}\">{HtmlEncode(multilineCodeBlockNode.Code)}</div>";
            }

            // Mention node
            if (node is MentionNode mentionNode)
            {
                // Meta mention node
                if (mentionNode.Type == MentionType.Meta)
                {
                    return $"<span class=\"mention\">@{HtmlEncode(mentionNode.Id)}</span>";
                }

                // User mention node
                if (mentionNode.Type == MentionType.User)
                {
                    var user = _chatLog.Mentionables.GetUser(mentionNode.Id);
                    return $"<span class=\"mention\" title=\"{HtmlEncode(user.FullName)}\">@{HtmlEncode(user.Name)}</span>";
                }

                // Channel mention node
                if (mentionNode.Type == MentionType.Channel)
                {
                    var channel = _chatLog.Mentionables.GetChannel(mentionNode.Id);
                    return $"<span class=\"mention\">#{HtmlEncode(channel.Name)}</span>";
                }

                // Role mention node
                if (mentionNode.Type == MentionType.Role)
                {
                    var role = _chatLog.Mentionables.GetRole(mentionNode.Id);
                    return $"<span class=\"mention\">@{HtmlEncode(role.Name)}</span>";
                }
            }

            // Emoji node
            if (node is EmojiNode emojiNode)
            {
                // Get emoji image URL
                var emojiImageUrl = Emoji.GetImageUrl(emojiNode.Id, emojiNode.Name, emojiNode.IsAnimated);

                // Make emoji large if it's jumbo
                var jumboableCssClass = isJumbo ? "emoji--large" : null;

                return $"<img class=\"emoji {jumboableCssClass}\" alt=\"{emojiNode.Name}\" title=\"{emojiNode.Name}\" src=\"{emojiImageUrl}\" />";
            }

            // Link node
            if (node is LinkNode linkNode)
            {
                // Extract message ID if the link points to a Discord message
                var linkedMessageId = Regex.Match(linkNode.Url, "^https?://discordapp.com/channels/.*?/(\\d+)/?$").Groups[1].Value;

                return linkedMessageId.IsNullOrWhiteSpace()
                    ? $"<a href=\"{Uri.EscapeUriString(linkNode.Url)}\">{HtmlEncode(linkNode.Title)}</a>"
                    : $"<a href=\"{Uri.EscapeUriString(linkNode.Url)}\" onclick=\"scrollToMessage(event, '{linkedMessageId}')\">{HtmlEncode(linkNode.Title)}</a>";
            }

            // All other nodes - simply return source
            return node.Source;
        }

        private string FormatMarkdown(IReadOnlyList<Node> nodes, bool isTopLevel)
        {
            // Emojis are jumbo if all top-level nodes are emoji nodes, disregarding whitespace
            var isJumbo = isTopLevel && nodes.Where(n => !n.Source.IsNullOrWhiteSpace()).All(n => n is EmojiNode);

            return nodes.Select(n => FormatMarkdown(n, isJumbo)).JoinToString("");
        }

        private string FormatMarkdown(string markdown) => FormatMarkdown(MarkdownParser.Parse(markdown), true);

        public async Task RenderAsync(TextWriter writer)
        {
            // Create template loader
            var loader = new TemplateLoader();

            // Get template
            var templateCode = loader.Load($"Html{_themeName}.html");
            var template = Template.Parse(templateCode);

            // Create template context
            var context = new TemplateContext
            {
                TemplateLoader = loader,
                MemberRenamer = m => m.Name,
                MemberFilter = m => true,
                LoopLimit = int.MaxValue,
                StrictVariables = true
            };

            // Create template model
            var model = new ScriptObject();
            model.SetValue("Model", _chatLog, true);
            model.Import(nameof(GroupMessages), new Func<IEnumerable<Message>, IEnumerable<MessageGroup>>(GroupMessages));
            model.Import(nameof(FormatDate), new Func<DateTimeOffset, string>(FormatDate));
            model.Import(nameof(FormatMarkdown), new Func<string, string>(FormatMarkdown));
            context.PushGlobal(model);

            // Configure output
            context.PushOutput(new TextWriterOutput(writer));

            // HACK: Render output in a separate thread
            // (even though Scriban has async API, it still makes a lot of blocking CPU-bound calls)
            await Task.Run(async () => await context.EvaluateAsync(template.Page));
        }
    }
}