﻿using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class Message : IMentionable
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

        public List<User> MentionedUsers { get; }

        public List<Role> MentionedRoles { get; }

        public List<Channel> MentionedChannels { get; }

        public Message(string id, string channelId, MessageType type,
            User author, DateTime timeStamp,
            DateTime? editedTimeStamp, string content,
            IReadOnlyList<Attachment> attachments, IReadOnlyList<Embed> embeds,
            List<User> mentionedUsers, List<Role> mentionedRoles, 
            List<Channel> mentionedChannels)
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