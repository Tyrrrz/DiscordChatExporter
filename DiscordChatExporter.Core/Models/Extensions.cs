using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public static class Extensions
    {
        public static string GetFileExtension(this ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return "txt";
            if (format == ExportFormat.HtmlDark)
                return "html";
            if (format == ExportFormat.HtmlLight)
                return "html";
            if (format == ExportFormat.Csv)
                return "csv";

            throw new ArgumentOutOfRangeException(nameof(format));
        }

        public static string GetDisplayName(this ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return "Plain Text";
            if (format == ExportFormat.HtmlDark)
                return "HTML (Dark)";
            if (format == ExportFormat.HtmlLight)
                return "HTML (Light)";
            if (format == ExportFormat.Csv)
                return "Comma Seperated Values (CSV)";

            throw new ArgumentOutOfRangeException(nameof(format));
        }

        public static IReadOnlyList<ChatLog> SplitIntoPartitions(this ChatLog chatLog, int partitionLimit)
        {
            // If chat log has fewer messages than the limit - just return chat log in a list
            if (chatLog.Messages.Count <= partitionLimit)
                return new[] {chatLog};

            var result = new List<ChatLog>();

            // Loop through messages
            var buffer = new List<Message>();
            foreach (var message in chatLog.Messages)
            {
                // Add message to buffer
                buffer.Add(message);

                // If reached the limit - split and reset buffer
                if (buffer.Count >= partitionLimit)
                {
                    // Add to result
                    var chatLogPartition = new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.From, chatLog.To, buffer,
                        chatLog.Mentionables);
                    result.Add(chatLogPartition);

                    // Reset the buffer instead of clearing to avoid mutations on existing references
                    buffer = new List<Message>();
                }
            }

            // Add what's remaining in buffer
            if (buffer.Any())
            {
                // Add to result
                var chatLogPartition = new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.From, chatLog.To, buffer,
                    chatLog.Mentionables);
                result.Add(chatLogPartition);
            }

            return result;
        }
    }
}