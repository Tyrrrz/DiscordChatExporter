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
    string? Url,
    DateTimeOffset? Timestamp,
    Color? Color,
    EmbedAuthor? Author,
    string? Description,
    IReadOnlyList<EmbedField> Fields,
    EmbedImage? Thumbnail,
    EmbedImage? Image,
    EmbedFooter? Footer)
{
    public PlainImageEmbedProjection? TryGetPlainImage() =>
        PlainImageEmbedProjection.TryResolve(this);

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
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTimeOffset();
        var color = json.GetPropertyOrNull("color")?.GetInt32OrNull()?.Pipe(System.Drawing.Color.FromArgb).ResetAlpha();
        var description = json.GetPropertyOrNull("description")?.GetStringOrNull();

        var author = json.GetPropertyOrNull("author")?.Pipe(EmbedAuthor.Parse);
        var thumbnail = json.GetPropertyOrNull("thumbnail")?.Pipe(EmbedImage.Parse);
        var image = json.GetPropertyOrNull("image")?.Pipe(EmbedImage.Parse);
        var footer = json.GetPropertyOrNull("footer")?.Pipe(EmbedFooter.Parse);

        var fields =
            json.GetPropertyOrNull("fields")?.EnumerateArrayOrNull()?.Select(EmbedField.Parse).ToArray() ??
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