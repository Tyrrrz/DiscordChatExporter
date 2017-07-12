using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class Message
    {
        public string Id { get; }

        public DateTime TimeStamp { get; }

        public User Author { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public Message(string id, DateTime timeStamp, User author, string content, IEnumerable<Attachment> attachments)
        {
            Id = id;
            TimeStamp = timeStamp;
            Author = author;
            Content = content;
            Attachments = attachments.ToArray();
        }

        public override string ToString()
        {
            return Content;
        }
    }
}