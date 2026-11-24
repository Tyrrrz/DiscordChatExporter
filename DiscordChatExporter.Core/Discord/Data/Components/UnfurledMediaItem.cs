using System;
using System.IO;
using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#unfurled-media-item
public record UnfurledMediaItem(
    string Url,
    string? ProxyUrl,
    int? Width,
    int? Height,
    string? ContentType,
    Snowflake? AttachmentId
)
{
    public string FileExtension { get; } = TryGetFileExtension(Url);

    public bool IsImage =>
        ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
        ?? (
            string.Equals(FileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".png", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".bmp", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".webp", StringComparison.OrdinalIgnoreCase)
        );

    public bool IsVideo =>
        ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
        ?? (
            string.Equals(FileExtension, ".gifv", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".mp4", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".webm", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".mov", StringComparison.OrdinalIgnoreCase)
        );

    public bool IsAudio =>
        ContentType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
        ?? (
            string.Equals(FileExtension, ".mp3", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".wav", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".ogg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".flac", StringComparison.OrdinalIgnoreCase)
            || string.Equals(FileExtension, ".m4a", StringComparison.OrdinalIgnoreCase)
        );

    private static string TryGetFileExtension(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "";

        var path = string.Equals(uri.Scheme, "attachment", StringComparison.OrdinalIgnoreCase)
            ? uri.Host
            : uri.AbsolutePath;

        return Path.GetExtension(path);
    }

    /// <summary>
    /// Whether this media item points at the specified message attachment.
    /// </summary>
    public bool Refers(Attachment attachment) =>
        AttachmentId == attachment.Id
        || string.Equals(Url, attachment.Url, StringComparison.Ordinal)
        || string.Equals(
            Url,
            $"attachment://{attachment.FileName}",
            StringComparison.OrdinalIgnoreCase
        );

    public static UnfurledMediaItem? TryParse(JsonElement json)
    {
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        if (url is null)
            return null;

        var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
        var contentType = json.GetPropertyOrNull("content_type")?.GetNonWhiteSpaceStringOrNull();
        var attachmentId = json.GetPropertyOrNull("attachment_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(s => Snowflake.TryParse(s));

        return new UnfurledMediaItem(url, proxyUrl, width, height, contentType, attachmentId);
    }
}
