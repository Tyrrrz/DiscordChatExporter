using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Ast;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering.Logic
{
    internal static class HtmlRenderingLogic
    {
        public static bool CanBeGrouped(Message message1, Message message2)
        {
            if (message1.Author.Id != message2.Author.Id)
                return false;

            if ((message2.Timestamp - message1.Timestamp).Duration().TotalMinutes > 7)
                return false;

            return true;
        }

        private static string HtmlEncode(string s) => WebUtility.HtmlEncode(s);

        private static string FormatMarkdownNode(RenderContext context, Node node, bool isJumbo)
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
                var innerHtml = FormatMarkdownNodes(context, formattedNode.Children, false);

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

                // Quote
                if (formattedNode.Formatting == TextFormatting.Quote)
                    return $"<div class=\"quote\">{innerHtml}</div>";
            }

            // Inline code block node
            if (node is InlineCodeBlockNode inlineCodeBlockNode)
            {
                return $"<span class=\"pre pre--inline\">{HtmlEncode(inlineCodeBlockNode.Code)}</span>";
            }

            // Multi-line code block node
            if (node is MultiLineCodeBlockNode multilineCodeBlockNode)
            {
                // Set CSS class for syntax highlighting
                var highlightCssClass = !string.IsNullOrWhiteSpace(multilineCodeBlockNode.Language)
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
                    var user = context.MentionableUsers.FirstOrDefault(u => u.Id == mentionNode.Id) ??
                               User.CreateUnknownUser(mentionNode.Id);

                    return $"<span class=\"mention\" title=\"{HtmlEncode(user.FullName)}\">@{HtmlEncode(user.Name)}</span>";
                }

                // Channel mention node
                if (mentionNode.Type == MentionType.Channel)
                {
                    var channel = context.MentionableChannels.FirstOrDefault(c => c.Id == mentionNode.Id) ??
                                  Channel.CreateDeletedChannel(mentionNode.Id);

                    return $"<span class=\"mention\">#{HtmlEncode(channel.Name)}</span>";
                }

                // Role mention node
                if (mentionNode.Type == MentionType.Role)
                {
                    var role = context.MentionableRoles.FirstOrDefault(r => r.Id == mentionNode.Id) ??
                               Role.CreateDeletedRole(mentionNode.Id);

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

                return string.IsNullOrWhiteSpace(linkedMessageId)
                    ? $"<a href=\"{Uri.EscapeUriString(linkNode.Url)}\">{HtmlEncode(linkNode.Title)}</a>"
                    : $"<a href=\"{Uri.EscapeUriString(linkNode.Url)}\" onclick=\"scrollToMessage(event, '{linkedMessageId}')\">{HtmlEncode(linkNode.Title)}</a>";
            }

            // Throw on unexpected nodes
            throw new InvalidOperationException($"Unexpected node [{node.GetType()}].");
        }

        private static string FormatMarkdownNodes(RenderContext context, IReadOnlyList<Node> nodes, bool isTopLevel)
        {
            // Emojis are jumbo if all top-level nodes are emoji nodes or whitespace text nodes
            var isJumbo = isTopLevel && nodes.All(n => n is EmojiNode || n is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Text));

            return nodes.Select(n => FormatMarkdownNode(context, n, isJumbo)).JoinToString("");
        }

        public static string FormatMarkdown(RenderContext context, string markdown) =>
            FormatMarkdownNodes(context, MarkdownParser.Parse(markdown), true);
    }
}