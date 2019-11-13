using System;
using System.Collections.Generic;
using System.Drawing;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object

    public class Embed
    {
        public string? Title { get; }

        public string? Url { get; }

        public DateTimeOffset? Timestamp { get; }

        // TODO: this should be nullable and default color should be set in CSS
        public Color Color { get; }

        public EmbedAuthor? Author { get; }

        public string? Description { get; }

        public IReadOnlyList<EmbedField> Fields { get; }

        public EmbedImage? Thumbnail { get; }

        public EmbedImage? Image { get; }

        public EmbedFooter? Footer { get; }

        public Embed(string? title, string? url, DateTimeOffset? timestamp, Color color, EmbedAuthor? author, string? description,
            IReadOnlyList<EmbedField> fields, EmbedImage? thumbnail, EmbedImage? image, EmbedFooter? footer)
        {
            Title = title;
            Url = url;
            Timestamp = timestamp;
            Color = color;
            Author = author;
            Description = description;
            Fields = fields;
            Thumbnail = thumbnail;
            Image = image;
            Footer = footer;
        }

        public override string ToString() => Title ?? "<untitled embed>";
    }
}