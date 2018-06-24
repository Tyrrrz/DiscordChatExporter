using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class MessageGroup
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

        public override string ToString() => $"{Author.FullName} | {Timestamp} | {Messages.Count} messages";
    }
}