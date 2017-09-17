using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class Message
    {
        public string Id { get; }

        public DateTime TimeStamp { get; }

        public DateTime? EditedTimeStamp { get; }

        public User Author { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public Message(string id, DateTime timeStamp, DateTime? editedTimeStamp, User author, string content,
            IEnumerable<Attachment> attachments)
        {
            Id = id;
            TimeStamp = timeStamp;
            EditedTimeStamp = editedTimeStamp;
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