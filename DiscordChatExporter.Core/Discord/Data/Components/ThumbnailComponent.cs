using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#thumbnail
public record ThumbnailComponent(UnfurledMediaItem? Media, string? Description, bool IsSpoiler)
    : MessageComponent(MessageComponentKind.Thumbnail)
{
    public static ThumbnailComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("media")?.Pipe(UnfurledMediaItem.TryParse),
            json.GetPropertyOrNull("description")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
}
