using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Nodes;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Internal;
using Tyrrrz.Extensions;

using static DiscordChatExporter.Core.Rendering.Logic.SharedRenderingLogic;

namespace DiscordChatExporter.Core.Rendering.Logic
{
    public static class PlainTextRenderingLogic
    {
        public static string FormatPreamble(RenderContext context)
        {
            var buffer = new StringBuilder();

            buffer.AppendLine('='.Repeat(62));
            buffer.AppendLine($"Guild: {context.Guild.Name}");
            buffer.AppendLine($"Channel: {context.Channel.Name}");

            if (!string.IsNullOrWhiteSpace(context.Channel.Topic))
                buffer.AppendLine($"Topic: {context.Channel.Topic}");

            if (context.After != null)
                buffer.AppendLine($"After: {FormatDate(context.After.Value, context.DateFormat)}");

            if (context.Before != null)
                buffer.AppendLine($"Before: {FormatDate(context.Before.Value, context.DateFormat)}");

            buffer.AppendLine('='.Repeat(62));

            return buffer.ToString();
        }

        private static string FormatMarkdownNode(RenderContext context, Node node)
        {
            // Text node
            if (node is TextNode textNode)
            {
                return textNode.Text;
            }

            // Mention node
            if (node is MentionNode mentionNode)
            {
                // Meta mention node
                if (mentionNode.Type == MentionType.Meta)
                {
                    return $"@{mentionNode.Id}";
                }

                // User mention node
                if (mentionNode.Type == MentionType.User)
                {
                    var user = context.MentionableUsers.FirstOrDefault(u => u.Id == mentionNode.Id) ??
                               User.CreateUnknownUser(mentionNode.Id);

                    return $"@{user.Name}";
                }

                // Channel mention node
                if (mentionNode.Type == MentionType.Channel)
                {
                    var channel = context.MentionableChannels.FirstOrDefault(c => c.Id == mentionNode.Id) ??
                                  Channel.CreateDeletedChannel(mentionNode.Id);

                    return $"#{channel.Name}";
                }

                // Role mention node
                if (mentionNode.Type == MentionType.Role)
                {
                    var role = context.MentionableRoles.FirstOrDefault(r => r.Id == mentionNode.Id) ??
                               Role.CreateDeletedRole(mentionNode.Id);

                    return $"@{role.Name}";
                }
            }

            // Emoji node
            if (node is EmojiNode emojiNode)
            {
                return emojiNode.IsCustomEmoji ? $":{emojiNode.Name}:" : emojiNode.Name;
            }

            // Throw on unexpected nodes
            throw new InvalidOperationException($"Unexpected node [{node.GetType()}].");
        }

        public static string FormatMarkdown(RenderContext context, string markdown) =>
            MarkdownParser.ParseMinimal(markdown).Select(n => FormatMarkdownNode(context, n)).JoinToString("");

        public static string FormatMessageHeader(RenderContext context, Message message)
        {
            var buffer = new StringBuilder();

            // Timestamp & author
            buffer
                .Append($"[{FormatDate(message.Timestamp, context.DateFormat)}]")
                .Append(' ')
                .Append($"{message.Author.FullName}");

            // Whether the message is pinned
            if (message.IsPinned)
            {
                buffer.Append(' ').Append("(pinned)");
            }

            return buffer.ToString();
        }

        public static string FormatMessageContent(RenderContext context, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                return "";

            return FormatMarkdown(context, message.Content);
        }

        public static string FormatAttachments(RenderContext context, IReadOnlyList<Attachment> attachments)
        {
            if (!attachments.Any())
                return "";

            var buffer = new StringBuilder();

            buffer
                .AppendLine("{Attachments}")
                .AppendJoin(Environment.NewLine, attachments.Select(a => a.Url))
                .AppendLine();

            return buffer.ToString();
        }

        public static string FormatEmbeds(RenderContext context, IReadOnlyList<Embed> embeds)
        {
            if (!embeds.Any())
                return "";

            var buffer = new StringBuilder();

            foreach (var embed in embeds)
            {
                buffer.AppendLine("{Embed}");

                // Author name
                if (!string.IsNullOrWhiteSpace(embed.Author?.Name))
                    buffer.AppendLine(embed.Author.Name);

                // URL
                if (!string.IsNullOrWhiteSpace(embed.Url))
                    buffer.AppendLine(embed.Url);

                // Title
                if (!string.IsNullOrWhiteSpace(embed.Title))
                    buffer.AppendLine(FormatMarkdown(context, embed.Title));

                // Description
                if (!string.IsNullOrWhiteSpace(embed.Description))
                    buffer.AppendLine(FormatMarkdown(context, embed.Description));

                // Fields
                foreach (var field in embed.Fields)
                {
                    // Name
                    if (!string.IsNullOrWhiteSpace(field.Name))
                        buffer.AppendLine(field.Name);

                    // Value
                    if (!string.IsNullOrWhiteSpace(field.Value))
                        buffer.AppendLine(field.Value);
                }

                // Thumbnail URL
                if (!string.IsNullOrWhiteSpace(embed.Thumbnail?.Url))
                    buffer.AppendLine(embed.Thumbnail?.Url);

                // Image URL
                if (!string.IsNullOrWhiteSpace(embed.Image?.Url))
                    buffer.AppendLine(embed.Image?.Url);

                // Footer text
                if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
                    buffer.AppendLine(embed.Footer?.Text);

                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        public static string FormatReactions(RenderContext context, IReadOnlyList<Reaction> reactions)
        {
            if (!reactions.Any())
                return "";

            var buffer = new StringBuilder();

            buffer.AppendLine("{Reactions}");

            foreach (var reaction in reactions)
            {
                buffer.Append(reaction.Emoji.Name);

                if (reaction.Count > 1)
                    buffer.Append($" ({reaction.Count})");

                buffer.Append(" ");
            }

            buffer.AppendLine();

            return buffer.ToString();
        }

        public static string FormatMessage(RenderContext context, Message message)
        {
            var buffer = new StringBuilder();

            buffer
                .AppendLine(FormatMessageHeader(context, message))
                .AppendLineIfNotEmpty(FormatMessageContent(context, message))
                .AppendLine()
                .AppendLineIfNotEmpty(FormatAttachments(context, message.Attachments))
                .AppendLineIfNotEmpty(FormatEmbeds(context, message.Embeds))
                .AppendLineIfNotEmpty(FormatReactions(context, message.Reactions));

            return buffer.Trim().ToString();
        }
    }
}