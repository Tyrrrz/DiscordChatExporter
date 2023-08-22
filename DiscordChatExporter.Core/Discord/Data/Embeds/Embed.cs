using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object
public partial record Embed(
    string? Title,
    EmbedKind Kind,
    string? Url,
    DateTimeOffset? Timestamp,
    Color? Color,
    EmbedAuthor? Author,
    string? Description,
    IReadOnlyList<EmbedField> Fields,
    EmbedImage? Thumbnail,
    IReadOnlyList<EmbedImage> Images,
    EmbedVideo? Video,
    EmbedFooter? Footer
)
{
    // Embeds can only have one image according to the API model,
    // but the client can render multiple images in some cases.
    public EmbedImage? Image => Images.FirstOrDefault();

    public SpotifyTrackEmbedProjection? TryGetSpotifyTrack() =>
        SpotifyTrackEmbedProjection.TryResolve(this);

    public YouTubeVideoEmbedProjection? TryGetYouTubeVideo() =>
        YouTubeVideoEmbedProjection.TryResolve(this);
}

public partial record Embed
{
    public static Embed Parse(JsonElement json)
    {
        var title = json.GetPropertyOrNull("title")?.GetStringOrNull();

        var kind =
            json.GetPropertyOrNull("type")?.GetStringOrNull()?.ParseEnumOrNull<EmbedKind>()
            ?? EmbedKind.Rich;

        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTimeOffsetOrNull();

        var color = json.GetPropertyOrNull("color")
            ?.GetInt32OrNull()
            ?.Pipe(System.Drawing.Color.FromArgb)
            .ResetAlpha();

        var author = json.GetPropertyOrNull("author")?.Pipe(EmbedAuthor.Parse);
        var description = json.GetPropertyOrNull("description")?.GetStringOrNull();

        var fields =
            json.GetPropertyOrNull("fields")
                ?.EnumerateArrayOrNull()
                ?.Select(EmbedField.Parse)
                .ToArray() ?? Array.Empty<EmbedField>();

        var thumbnail = json.GetPropertyOrNull("thumbnail")?.Pipe(EmbedImage.Parse);

        // Under the Discord API model, embeds can only have at most one image.
        // Because of that, embeds that are rendered with multiple images on the client
        // (e.g. tweet embeds), are exposed from the API as multiple separate embeds.
        // Our embed model is consistent with the user-facing side of Discord, so images
        // are stored as an array. The API will only ever return one image, but we deal
        // with this by merging related embeds at the end of the message parsing process.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/695
        var images =
            json.GetPropertyOrNull("image")
                ?.Pipe(EmbedImage.Parse)
                .ToSingletonEnumerable()
                .ToArray() ?? Array.Empty<EmbedImage>();

        var video = json.GetPropertyOrNull("video")?.Pipe(EmbedVideo.Parse);

        var footer = json.GetPropertyOrNull("footer")?.Pipe(EmbedFooter.Parse);

        return new Embed(
            title,
            kind,
            url,
            timestamp,
            color,
            author,
            description,
            fields,
            thumbnail,
            images,
            video,
            footer
        );
    }
}
