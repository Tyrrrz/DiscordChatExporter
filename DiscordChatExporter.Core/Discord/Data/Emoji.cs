using System;
using System.Linq;
using System.Text.Json;
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
    string ImageUrl)
{
    // Name of a custom emoji (e.g. LUL) or name of a standard emoji (e.g. slight_smile)
    public string Code => Id is not null
        ? Name
        : EmojiIndex.TryGetCode(Name) ?? Name;
}

public partial record Emoji
{
    private static string GetTwemojiId(string name)
    {
        var runes = name.GetRunes().ToArray();

        // Variant selector rune is skipped in Twemoji names, except when the emoji also contains a zero-width joiner.
        // VS = 0xfe0f; ZWJ = 0x200d.
        var filteredRunes = runes.Any(r => r.Value == 0x200d)
            ? runes
            : runes.Where(r => r.Value != 0xfe0f);

        return string.Join(
            "-",
            filteredRunes.Select(r => r.Value.ToString("x"))
        );
    }

    private static string GetImageUrl(Snowflake id, bool isAnimated) => isAnimated
        ? $"https://cdn.discordapp.com/emojis/{id}.gif"
        : $"https://cdn.discordapp.com/emojis/{id}.png";

    private static string GetImageUrl(string name) =>
        $"https://cdn.jsdelivr.net/gh/twitter/twemoji@latest/assets/svg/{GetTwemojiId(name)}.svg";

    public static string GetImageUrl(Snowflake? id, string? name, bool isAnimated)
    {
        // Custom emoji
        if (id is not null)
            return GetImageUrl(id.Value, isAnimated);

        // Standard emoji
        if (!string.IsNullOrWhiteSpace(name))
            return GetImageUrl(name);

        // Either ID or name should be set
        throw new ApplicationException("Emoji has neither ID nor name set.");
    }

    public static Emoji Parse(JsonElement json)
    {
        var id = json.GetPropertyOrNull("id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse);
        var name = json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull();
        var isAnimated = json.GetPropertyOrNull("animated")?.GetBooleanOrNull() ?? false;
        var imageUrl = GetImageUrl(id, name, isAnimated);

        return new Emoji(
            id,
            // Name may be missing if it's an emoji inside a reaction
            name ?? "<unknown emoji>",
            isAnimated,
            imageUrl
        );
    }
}