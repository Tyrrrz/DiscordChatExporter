using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private class MessageGroup
        {
            public User Author { get; }

            public DateTime Timestamp { get; }

            public IReadOnlyList<Message> Messages { get; }

            public MessageGroup(User author, DateTime timestamp, IReadOnlyList<Message> messages)
            {
                Author = author;
                Timestamp = timestamp;
                Messages = messages;
            }
        }
    }
}