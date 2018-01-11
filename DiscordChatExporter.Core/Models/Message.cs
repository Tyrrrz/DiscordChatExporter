using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class Message
    {
        public string Id { get; }

        public MessageType Type { get; }

        public User Author { get; }

        public DateTime TimeStamp { get; }

        public DateTime? EditedTimeStamp { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public IReadOnlyList<User> MentionedUsers { get; }

        public IReadOnlyList<Role> MentionedRoles { get; }

        public IReadOnlyList<Channel> MentionedChannels { get; }

        public Message(string id, MessageType type, User author,
            DateTime timeStamp, DateTime? editedTimeStamp,
            string content, IReadOnlyList<Attachment> attachments,
            IReadOnlyList<User> mentionedUsers, IReadOnlyList<Role> mentionedRoles,
            IReadOnlyList<Channel> mentionedChannels)
        {
            Id = id;
            Type = type;
            Author = author;
            TimeStamp = timeStamp;
            EditedTimeStamp = editedTimeStamp;
            Content = content;
            Attachments = attachments;
            MentionedUsers = mentionedUsers;
            MentionedRoles = mentionedRoles;
            MentionedChannels = mentionedChannels;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}