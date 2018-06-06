using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object

    public class Embed
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

        public Embed(string title, string type, string description, 
            string url, DateTime? timeStamp, Color? color, 
            EmbedFooter footer, EmbedImage image, EmbedImage thumbnail, 
            EmbedVideo video, EmbedProvider provider, EmbedAuthor author, 
            IReadOnlyList<EmbedField> fields)
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
        }

        public override string ToString()
        {
            return Description;
        }
    }
}