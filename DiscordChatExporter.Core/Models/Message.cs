using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Models.Embeds;

namespace DiscordChatExporter.Core.Models
{
    public class Message
    {
        public string Id { get; }

        public string ChannelId { get; }

        public MessageType Type { get; }

        public User Author { get; }

        public DateTime TimeStamp { get; }

        public DateTime? EditedTimeStamp { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public IReadOnlyList<Embed> Embeds { get; }

        public MentionContainer Mentions { get; }

        public Message(string id, string channelId, MessageType type,
            User author, DateTime timeStamp,
            DateTime? editedTimeStamp, string content,
            IReadOnlyList<Attachment> attachments, IReadOnlyList<Embed> embeds,
            MentionContainer mentions)
        {
            Id = id;
            ChannelId = channelId;
            Type = type;
            Author = author;
            TimeStamp = timeStamp;
            EditedTimeStamp = editedTimeStamp;
            Content = content;
            Attachments = attachments;
            Embeds = embeds;
            Mentions = mentions;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}