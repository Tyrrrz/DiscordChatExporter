using DiscordChatExporter.Core.Models;
using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Render
{
    public partial class HtmlChatLogRenderer
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