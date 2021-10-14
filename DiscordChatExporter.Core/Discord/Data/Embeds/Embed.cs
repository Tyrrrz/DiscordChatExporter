using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object
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

        public PlainImageEmbedProjection? TryGetPlainImage() => PlainImageEmbedProjection.TryResolve(this);

        public TenorEmbedProjection? TryGetGif() => TenorEmbedProjection.TryResolve(this);

        public SpotifyTrackEmbedProjection? TryGetSpotifyTrack() => SpotifyTrackEmbedProjection.TryResolve(this);

        public YouTubeVideoEmbedProjection? TryGetYouTubeVideo() => YouTubeVideoEmbedProjection.TryResolve(this);

        [ExcludeFromCodeCoverage]
        public override string ToString() => Title ?? "<untitled embed>";
    }

    public partial class Embed
    {
        public static Embed Parse(JsonElement json)
        {
            var title = json.GetPropertyOrNull("title")?.GetString();
            var url = json.GetPropertyOrNull("url")?.GetString();
            var timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTimeOffset();
            var color = json.GetPropertyOrNull("color")?.GetInt32().Pipe(System.Drawing.Color.FromArgb).ResetAlpha();
            var description = json.GetPropertyOrNull("description")?.GetString();

            var author = json.GetPropertyOrNull("author")?.Pipe(EmbedAuthor.Parse);
            var thumbnail = json.GetPropertyOrNull("thumbnail")?.Pipe(EmbedImage.Parse);
            var image = json.GetPropertyOrNull("image")?.Pipe(EmbedImage.Parse);
            if(image is null)
            {
                try
                {
                    image = json.GetPropertyOrNull("video")?.Pipe(EmbedImage.Parse);
                }
                catch {} //meh
            }
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