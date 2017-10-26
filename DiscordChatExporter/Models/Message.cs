using System;
using System.Collections.Generic;

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

        public IReadOnlyList<User> MentionedUsers { get; }

        public IReadOnlyList<Role> MentionedRoles { get; }

        public Message(string id, User author,
            DateTime timeStamp, DateTime? editedTimeStamp,
            string content, IReadOnlyList<Attachment> attachments,
            IReadOnlyList<User> mentionedUsers, IReadOnlyList<Role> mentionedRoles)
        {
            Id = id;
            Author = author;
            TimeStamp = timeStamp;
            EditedTimeStamp = editedTimeStamp;
            Content = content;
            Attachments = attachments;
            MentionedUsers = mentionedUsers;
            MentionedRoles = mentionedRoles;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}