using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/emoji#emoji-object
public partial record Emoji(
    // Only present on custom emoji
    Snowflake? Id,
    // Name of a custom emoji (e.g. LUL) or actual representation of a standard emoji (e.g. 🙂)
    string Name,
    bool IsAnimated,
    string ImageUrl
)
{
    // Name of a custom emoji (e.g. LUL) or name of a standard emoji (e.g. slight_smile)
    public string Code => Id is not null ? Name : EmojiIndex.TryGetCode(Name) ?? Name;
}

public partial record Emoji
{
    public static string GetImageUrl(Snowflake? id, string? name, bool isAnimated)
    {
        // Custom emoji
        if (id is not null)
            return ImageCdn.GetCustomEmojiUrl(id.Value, isAnimated);

        // Standard emoji
        if (!string.IsNullOrWhiteSpace(name))
            return ImageCdn.GetStandardEmojiUrl(name);

        throw new InvalidOperationException("Either the emoji ID or name should be provided.");
    }

    public static Emoji Parse(JsonElement json)
    {
        var id = json.GetPropertyOrNull("id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        // Names may be missing on custom emoji within reactions
        var name =
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ?? "Unknown Emoji";

        var isAnimated = json.GetPropertyOrNull("animated")?.GetBooleanOrNull() ?? false;
        var imageUrl = GetImageUrl(id, name, isAnimated);

        return new Emoji(id, name, isAnimated, imageUrl);
    }
}
