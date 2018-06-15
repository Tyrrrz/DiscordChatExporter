using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object

    public class Embed
    {
        public string Title { get; }

        public string Description { get; }

        public string Url { get; }

        public DateTime? TimeStamp { get; }

        public Color Color { get; }

        public EmbedAuthor Author { get; }

        public IReadOnlyList<EmbedField> Fields { get; }

        public EmbedImage Thumbnail { get; }

        public EmbedImage Image { get; }

        public EmbedFooter Footer { get; }

        public Embed(string title, string description, string url, DateTime? timeStamp, Color color, EmbedFooter footer,
            EmbedImage image, EmbedImage thumbnail, EmbedAuthor author, IReadOnlyList<EmbedField> fields)
        {
            Title = title;
            Description = description;
            Url = url;
            TimeStamp = timeStamp;
            Color = color;
            Footer = footer;
            Image = image;
            Thumbnail = thumbnail;
            Author = author;
            Fields = fields;
        }

        public override string ToString() => Title;
    }
}