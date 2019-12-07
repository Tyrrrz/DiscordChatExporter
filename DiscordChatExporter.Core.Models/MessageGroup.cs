using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    // Used for grouping contiguous messages in HTML export

    public class MessageGroup
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