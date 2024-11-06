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
    bool IsAnimated
)
{
    public bool IsCustomEmoji { get; } = Id is not null;

    // Name of a custom emoji (e.g. LUL) or name of a standard emoji (e.g. slight_smile)
    public string Code { get; } = Id is not null ? Name : EmojiIndex.TryGetCode(Name) ?? Name;

    public string ImageUrl { get; } =
        Id is not null
            ? ImageCdn.GetCustomEmojiUrl(Id.Value, IsAnimated)
            : ImageCdn.GetStandardEmojiUrl(Name);
}

public partial record Emoji
{
    public static Emoji Parse(JsonElement json)
    {
        var id = json.GetPropertyOrNull("id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        // Names may be missing on custom emoji within reactions
        var name =
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ?? "Unknown Emoji";

        var isAnimated = json.GetPropertyOrNull("animated")?.GetBooleanOrNull() ?? false;

        return new Emoji(id, name, isAnimated);
    }
}
