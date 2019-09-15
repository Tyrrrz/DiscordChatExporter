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
    public class CsvChatLogRenderer : IChatLogRenderer
    {
        private readonly ChatLog _chatLog;
        private readonly string _dateFormat;

        public CsvChatLogRenderer(ChatLog chatLog, string dateFormat)
        {
            _chatLog = chatLog;
            _dateFormat = dateFormat;
        }

        private string FormatDate(DateTimeOffset date) =>
            date.ToLocalTime().ToString(_dateFormat, CultureInfo.InvariantCulture);

        private string FormatMarkdown(Node node)
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
                    return mentionNode.Id;
                }

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

            // Emoji node
            if (node is EmojiNode emojiNode)
            {
                return emojiNode.IsCustomEmoji ? $":{emojiNode.Name}:" : emojiNode.Name;
            }

            // Throw on unexpected nodes
            throw new InvalidOperationException($"Unexpected node: [{node.GetType()}].");
        }

        private string FormatMarkdown(IEnumerable<Node> nodes) => nodes.Select(FormatMarkdown).JoinToString("");

        private string FormatMarkdown(string markdown) => FormatMarkdown(MarkdownParser.ParseMinimal(markdown));

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Author ID
            await RenderFieldAsync(writer, message.Author.Id);

            // Author
            await RenderFieldAsync(writer, message.Author.FullName);

            // Timestamp
            await RenderFieldAsync(writer, FormatDate(message.Timestamp));

            // Content
            await RenderFieldAsync(writer, FormatMarkdown(message.Content));

            // Attachments
            var formattedAttachments = message.Attachments.Select(a => a.Url).JoinToString(",");
            await RenderFieldAsync(writer, formattedAttachments);

            // Reactions
            var formattedReactions = message.Reactions.Select(r => $"{r.Emoji.Name} ({r.Count})").JoinToString(",");
            await RenderFieldAsync(writer, formattedReactions);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("AuthorID;Author;Date;Content;Attachments;Reactions;");

            // Log
            foreach (var message in _chatLog.Messages)
                await RenderMessageAsync(writer, message);
        }
    }
}