using System;
using System.Collections.Generic;
using System.Drawing;

// https://discordapp.com/developers/docs/resources/channel#embed-object

namespace DiscordChatExporter.Core.Models
{
    public class Embed : IMentionable
    {
        public string Title { get; }

        public string Type { get; }

        public string Description { get; }

        public string Url { get; }

        public DateTime? TimeStamp { get; }

        public Color? Color { get; }

        public EmbedFooter Footer { get; }

        public EmbedImage Image { get; }

        public EmbedImage Thumbnail { get; }

        public EmbedVideo Video { get; }

        public EmbedProvider Provider { get; }

        public EmbedAuthor Author { get; }

        public IReadOnlyList<EmbedField> Fields { get; }

        public IReadOnlyList<User> MentionedUsers { get; }

        public IReadOnlyList<Role> MentionedRoles { get; }

        public IReadOnlyList<Channel> MentionedChannels { get; }

        public Embed(string title, string type, string description, 
            string url, DateTime? timeStamp, Color? color, 
            EmbedFooter footer, EmbedImage image, EmbedImage thumbnail, 
            EmbedVideo video, EmbedProvider provider, EmbedAuthor author, 
            IReadOnlyList<EmbedField> fields, IReadOnlyList<User> mentionedUsers,
            IReadOnlyList<Role> mentionedRoles, IReadOnlyList<Channel> mentionedChannels)
        {
            Title = title;
            Type = type;
            Description = description;
            Url = url;
            TimeStamp = timeStamp;
            Color = color;
            Footer = footer;
            Image = image;
            Thumbnail = thumbnail;
            Video = video;
            Provider = provider;
            Author = author;
            Fields = fields;
            MentionedUsers = mentionedUsers;
            MentionedRoles = mentionedRoles;
            MentionedChannels = mentionedChannels;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}