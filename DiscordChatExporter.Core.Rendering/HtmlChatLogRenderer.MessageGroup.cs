using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public partial class HtmlChatLogRenderer
    {
        private class MessageGroup
        {
            public User Author { get; }

            public DateTimeOffset Timestamp { get; }

            public IReadOnlyList<Message> Messages { get; }

            public MessageGroup(User author, DateTimeOffset timestamp, IReadOnlyList<Message> messages)
            {
                Author = author;
                Timestamp = timestamp;
                Messages = messages;
            }
        }
    }
}