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

        private async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Timestamp and author
            await writer.WriteLineAsync($"[{FormatDate(message.Timestamp)}] {message.Author.FullName}");

            // Content
            await writer.WriteLineAsync(FormatMarkdown(message.Content));

            // Attachments
            foreach (var attachment in message.Attachments)
                await writer.WriteLineAsync(attachment.Url);
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
            {
                await RenderMessageAsync(writer, message);
                await writer.WriteLineAsync();
            }
        }
    }
}