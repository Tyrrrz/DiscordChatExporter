using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class MessageGroup
    {
        public User Author { get; }

        public DateTime TimeStamp { get; }

        public IReadOnlyList<Message> Messages { get; }

        public MessageGroup(User author, DateTime timeStamp, IReadOnlyList<Message> messages)
        {
            Author = author;
            TimeStamp = timeStamp;
            Messages = messages;
        }

        public override string ToString() => $"{Author.FullName} | {TimeStamp} | {Messages.Count} messages";
    }
}