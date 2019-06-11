using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Nodes;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    public class PlainTextChatLogRenderer : IChatLogRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;

        public PlainTextChatLogRenderer(ChatLog chatLog, string dateFormat)
        {
            _chatLog = chatLog;
            _dateFormat = dateFormat;
        }

        private string FormatDate(DateTimeOffset date) =>
            date.ToLocalTime().ToString(_dateFormat, CultureInfo.InvariantCulture);

        private string FormatDateRange(DateTimeOffset? after, DateTimeOffset? before)
        {
            // Both 'after' and 'before'
            if (after != null && before != null)
                return $"{FormatDate(after.Value)} to {FormatDate(before.Value)}";

            // Just 'after'
            if (after != null)
                return $"after {FormatDate(after.Value)}";

            // Just 'before'
            if (before != null)
                return $"before {FormatDate(before.Value)}";

            // Neither
            return null;
        }

        private string FormatMarkdown(Node node)
        {
            // Formatted node
            if (node is FormattedNode formattedNode)
            {
                // Recursively get inner text
                var innerText = FormatMarkdown(formattedNode.Children);

                return $"{formattedNode.Token}{innerText}{formattedNode.Token}";
            }

            // Non-meta mention node
            if (node is MentionNode mentionNode && mentionNode.Type != MentionType.Meta)
            {
                // User mention node
                if (mentionNode.Type == MentionType.User)
                {
                    var user = _chatLog.Mentionables.GetUser(mentionNode.Id);
                    return $"@{user.Name}";
                }

                // Channel mention node
                if (mentionNode.Type == MentionType.Channel)
                {
                    var channel = _chatLog.Mentionables.GetChannel(mentionNode.Id);
                    return $"#{channel.Name}";
                }

                // Role mention node
                if (mentionNode.Type == MentionType.Role)
                {
                    var role = _chatLog.Mentionables.GetRole(mentionNode.Id);
                    return $"@{role.Name}";
                }
            }

            // Custom emoji node
            if (node is EmojiNode emojiNode && emojiNode.IsCustomEmoji)
            {
                return $":{emojiNode.Name}:";
            }

            // All other nodes - simply return source
            return node.Source;
        }

        private string FormatMarkdown(IEnumerable<Node> nodes) => nodes.Select(FormatMarkdown).JoinToString("");

        private string FormatMarkdown(string markdown) => FormatMarkdown(MarkdownParser.Parse(markdown));

        private async Task RenderAttachmentsAsync(TextWriter writer, IReadOnlyList<Attachment> attachments)
        {
            if (attachments.Any())
            {
                await writer.WriteLineAsync("{Attachments}");

                foreach (var attachment in attachments)
                    await writer.WriteLineAsync(attachment.Url);

                await writer.WriteLineAsync();
            }
        }

        private async Task RenderEmbedsAsync(TextWriter writer, IReadOnlyList<Embed> embeds)
        {
            foreach (var embed in embeds)
            {
                await writer.WriteLineAsync("{Embed}");

                // Author name
                if (!(embed.Author?.Name).IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(embed.Author?.Name);

                // URL
                if (!embed.Url.IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(embed.Url);

                // Title
                if (!embed.Title.IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(FormatMarkdown(embed.Title));

                // Description
                if (!embed.Description.IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(FormatMarkdown(embed.Description));

                // Fields
                foreach (var field in embed.Fields)
                {
                    // Name
                    if (!field.Name.IsNullOrWhiteSpace())
                        await writer.WriteLineAsync(field.Name);

                    // Value
                    if (!field.Value.IsNullOrWhiteSpace())
                        await writer.WriteLineAsync(field.Value);
                }

                // Thumbnail URL
                if (!(embed.Thumbnail?.Url).IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(embed.Thumbnail?.Url);

                // Image URL
                if (!(embed.Image?.Url).IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(embed.Image?.Url);

                // Footer text
                if (!(embed.Footer?.Text).IsNullOrWhiteSpace())
                    await writer.WriteLineAsync(embed.Footer?.Text);

                await writer.WriteLineAsync();
            }
        }

        private async Task RenderReactionsAsync(TextWriter writer, IReadOnlyList<Reaction> reactions)
        {
            if (reactions.Any())
            {
                await writer.WriteLineAsync("{Reactions}");

                foreach (var reaction in reactions)
                {
                    await writer.WriteAsync(reaction.Emoji.Name);

                    if (reaction.Count > 1)
                        await writer.WriteAsync($" ({reaction.Count})");

                    await writer.WriteAsync(" ");
                }

                await writer.WriteLineAsync();
                await writer.WriteLineAsync();
            }
        }

        private async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Timestamp and author
            await writer.WriteLineAsync($"[{FormatDate(message.Timestamp)}] {message.Author.FullName}");

            // Content
            await writer.WriteLineAsync(FormatMarkdown(message.Content));

            // Separator
            await writer.WriteLineAsync();

            // Attachments
            await RenderAttachmentsAsync(writer, message.Attachments);

            // Embeds
            await RenderEmbedsAsync(writer, message.Embeds);

            // Reactions
            await RenderReactionsAsync(writer, message.Reactions);
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Metadata
            await writer.WriteLineAsync('='.Repeat(62));
            await writer.WriteLineAsync($"Guild: {_chatLog.Guild.Name}");
            await writer.WriteLineAsync($"Channel: {_chatLog.Channel.Name}");
            await writer.WriteLineAsync($"Topic: {_chatLog.Channel.Topic}");
            await writer.WriteLineAsync($"Messages: {_chatLog.Messages.Count:N0}");
            await writer.WriteLineAsync($"Range: {FormatDateRange(_chatLog.After, _chatLog.Before)}");
            await writer.WriteLineAsync('='.Repeat(62));
            await writer.WriteLineAsync();

            // Log
            foreach (var message in _chatLog.Messages)
                await RenderMessageAsync(writer, message);
        }
    }
}