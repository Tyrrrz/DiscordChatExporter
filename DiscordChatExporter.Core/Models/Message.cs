using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Models.Embeds;

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

        public Message(string id, string channelId, MessageType type,
            User author, DateTime timestamp,
            DateTime? editedTimestamp, string content,
            IReadOnlyList<Attachment> attachments, IReadOnlyList<Embed> embeds)
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
        }

        public override string ToString() => Content;
    }
}