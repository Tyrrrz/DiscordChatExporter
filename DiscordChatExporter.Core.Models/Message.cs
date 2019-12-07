using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#message-object

    public class Message : IHasId
    {
        public string Id { get; }

        public string ChannelId { get; }

        public MessageType Type { get; }

        public User Author { get; }

        public DateTimeOffset Timestamp { get; }

        public DateTimeOffset? EditedTimestamp { get; }

        public bool IsPinned { get; }

        public string? Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public IReadOnlyList<Embed> Embeds { get; }

        public IReadOnlyList<Reaction> Reactions { get; }

        public IReadOnlyList<User> MentionedUsers { get; }

        public Message(string id, string channelId, MessageType type, User author,
            DateTimeOffset timestamp, DateTimeOffset? editedTimestamp, bool isPinned,
            string content,
            IReadOnlyList<Attachment> attachments,IReadOnlyList<Embed> embeds, IReadOnlyList<Reaction> reactions,
            IReadOnlyList<User> mentionedUsers)
        {
            Id = id;
            ChannelId = channelId;
            Type = type;
            Author = author;
            Timestamp = timestamp;
            EditedTimestamp = editedTimestamp;
            IsPinned = isPinned;
            Content = content;
            Attachments = attachments;
            Embeds = embeds;
            Reactions = reactions;
            MentionedUsers = mentionedUsers;
        }

        public override string ToString() => Content ?? "<message without content>";
    }
}