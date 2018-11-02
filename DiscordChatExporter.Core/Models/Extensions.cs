using System;
using System.Collections.Generic;

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

        public static IReadOnlyList<ChatLog> SplitIntoPartitions(this ChatLog chatLog, int maxMessageCountPerPartition)
        {
            // If chat log has fewer messages than the limit - just return chat log in a list
            if (chatLog.Messages.Count <= maxMessageCountPerPartition)
                return new[] {chatLog};

            var result = new List<ChatLog>();

            // Loop through messages
            var messageBuffer = new List<Message>();
            foreach (var message in chatLog.Messages)
            {
                // Add message to buffer
                messageBuffer.Add(message);

                // If reached the limit - split and clear buffer
                if (messageBuffer.Count >= maxMessageCountPerPartition)
                {
                    // Add to result
                    var chatLogPartition = new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.From, chatLog.To,
                        messageBuffer, chatLog.Mentionables);
                    result.Add(chatLogPartition);

                    // Clear buffer
                    messageBuffer.Clear();
                }
            }

            return result;
        }
    }
}