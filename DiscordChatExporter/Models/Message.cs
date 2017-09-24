using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class Message
    {
        public string Id { get; }

        public User Author { get; }

        public DateTime TimeStamp { get; }

        public DateTime? EditedTimeStamp { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public Message(string id, User author,
            DateTime timeStamp, DateTime? editedTimeStamp,
            string content, IEnumerable<Attachment> attachments)
        {
            Id = id;
            Author = author;
            TimeStamp = timeStamp;
            EditedTimeStamp = editedTimeStamp;
            Content = content;
            Attachments = attachments.ToArray();
        }

        public override string ToString()
        {
            return Content;
        }
    }
}