﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class MessageGroup
    {
        public User Author { get; }

        public DateTime TimeStamp { get; }

        public IReadOnlyList<Message> Messages { get; }

        public MessageGroup(User author, DateTime timeStamp, IEnumerable<Message> messages)
        {
            Author = author;
            TimeStamp = timeStamp;
            Messages = messages.ToArray();
        }
    }
}