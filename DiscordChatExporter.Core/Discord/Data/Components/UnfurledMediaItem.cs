using System;
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
    public bool IsVideo => ContentType?.StartsWith("video/") == true;

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
