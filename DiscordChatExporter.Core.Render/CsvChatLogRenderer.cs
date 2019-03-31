using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Render
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

        private string FormatDate(DateTime date) => date.ToString(_dateFormat, CultureInfo.InvariantCulture);

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

            // All other nodes - simply return lexeme
            return node.Lexeme;
        }

        private string FormatMarkdown(IEnumerable<Node> nodes) => nodes.Select(FormatMarkdown).JoinToString("");

        private string FormatMarkdown(string markdown) => FormatMarkdown(MarkdownParser.Parse(markdown));

        private async Task RenderFieldAsync(TextWriter writer, string value)
        {
            var encodedValue = value.Replace("\"", "\"\"");
            await writer.WriteAsync($"\"{encodedValue}\";");
        }

        private async Task RenderMessageAsync(TextWriter writer, Message message)
        {
            // Author
            await RenderFieldAsync(writer, message.Author.FullName);

            // Timestamp
            await RenderFieldAsync(writer, FormatDate(message.Timestamp));

            // Content
            var formattedContent = await Task.Run(() => FormatMarkdown(message.Content));
            await RenderFieldAsync(writer, formattedContent);

            // Attachments
            var formattedAttachments = message.Attachments.Select(a => a.Url).JoinToString(",");
            await RenderFieldAsync(writer, formattedAttachments);

            // Line break
            await writer.WriteLineAsync();
        }

        public async Task RenderAsync(TextWriter writer)
        {
            // Headers
            await writer.WriteLineAsync("Author;Date;Content;Attachments;");

            // Log
            foreach (var message in _chatLog.Messages)
                await RenderMessageAsync(writer, message);
        }
    }
}