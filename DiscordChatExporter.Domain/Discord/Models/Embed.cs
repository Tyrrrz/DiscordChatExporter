using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object
    public partial class Embed
    {
        public string? Title { get; }

        public string? Url { get; }

        public DateTimeOffset? Timestamp { get; }

        public Color? Color { get; }

        public EmbedAuthor? Author { get; }

        public string? Description { get; }

        public IReadOnlyList<EmbedField> Fields { get; }

        public EmbedImage? Thumbnail { get; }

        public EmbedImage? Image { get; }

        public EmbedFooter? Footer { get; }

        public Embed(
            string? title,
            string? url,
            DateTimeOffset? timestamp,
            Color? color,
            EmbedAuthor? author,
            string? description,
            IReadOnlyList<EmbedField> fields,
            EmbedImage? thumbnail,
            EmbedImage? image,
            EmbedFooter? footer)
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

    public partial class Embed
    {
        public static Embed Parse(JsonElement json)
        {
            var title = json.GetPropertyOrNull("title")?.GetString();
            var description = json.GetPropertyOrNull("description")?.GetString();
            var url = json.GetPropertyOrNull("url")?.GetString();
            var timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTimeOffset();
            var color = json.GetPropertyOrNull("color")?.GetInt32().Pipe(System.Drawing.Color.FromArgb).ResetAlpha();

            var author = json.GetPropertyOrNull("author")?.Pipe(EmbedAuthor.Parse);
            var thumbnail = json.GetPropertyOrNull("thumbnail")?.Pipe(EmbedImage.Parse);
            var image = json.GetPropertyOrNull("image")?.Pipe(EmbedImage.Parse);
            var footer = json.GetPropertyOrNull("footer")?.Pipe(EmbedFooter.Parse);

            var fields =
                json.GetPropertyOrNull("fields")?.EnumerateArray().Select(EmbedField.Parse).ToArray() ??
                Array.Empty<EmbedField>();

            return new Embed(
                title,
                url,
                timestamp,
                color,
                author,
                description,
                fields,
                thumbnail,
                image,
                footer
            );
        }
    }
}