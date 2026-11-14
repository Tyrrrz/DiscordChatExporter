using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#media-gallery
public record MediaGalleryComponent(IReadOnlyList<MediaGalleryItem> Items)
    : MessageComponent(MessageComponentKind.MediaGallery)
{
    public static MediaGalleryComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("items")
                ?.EnumerateArrayOrNull()
                ?.Select(MediaGalleryItem.TryParse)
                .WhereNotNull()
                .ToArray()
                ?? []
        );
}
