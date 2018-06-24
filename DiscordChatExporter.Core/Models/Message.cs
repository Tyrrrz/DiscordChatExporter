using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#message-object

    public class Message
    {
        public string Id { get; }

        public string ChannelId { get; }

        public MessageType Type { get; }

        public User Author { get; }

        public DateTime Timestamp { get; }

        public DateTime? EditedTimestamp { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public IReadOnlyList<Embed> Embeds { get; }

        public IReadOnlyList<User> MentionedUsers { get; }

        public Message(string id, string channelId, MessageType type,
            User author, DateTime timestamp,
            DateTime? editedTimestamp, string content,
            IReadOnlyList<Attachment> attachments, IReadOnlyList<Embed> embeds, IReadOnlyList<User> mentionedUsers)
        {
            Id = id;
            ChannelId = channelId;
            Type = type;
            Author = author;
            Timestamp = timestamp;
            EditedTimestamp = editedTimestamp;
            Content = content;
            Attachments = attachments;
            Embeds = embeds;
            MentionedUsers = mentionedUsers;
        }

        public override string ToString() => Content;
    }
}