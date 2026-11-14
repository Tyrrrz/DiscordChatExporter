using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#media-gallery-media-gallery-item-structure
public record MediaGalleryItem(UnfurledMediaItem Media, string? Description, bool IsSpoiler)
{
    public static MediaGalleryItem? TryParse(JsonElement json)
    {
        var media = json.GetPropertyOrNull("media")?.Pipe(UnfurledMediaItem.TryParse);
        if (media is null)
            return null;

        return new MediaGalleryItem(
            media,
            json.GetPropertyOrNull("description")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
    }
}
