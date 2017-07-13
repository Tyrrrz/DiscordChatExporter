using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class MessageGroup
    {
        public MessageGroup(User author, DateTime firstTimeStamp, IEnumerable<Message> messages)
        {
            Author = author;
            FirstTimeStamp = firstTimeStamp;
            Messages = messages.ToArray();
        }

        public User Author { get; }

        public DateTime FirstTimeStamp { get; }

        public IReadOnlyList<Message> Messages { get; }
    }
}