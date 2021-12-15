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
    string? Id,
    // Name of custom emoji (e.g. LUL) or actual representation of standard emoji (e.g. 🙂)
    string Name,
    bool IsAnimated,
    string ImageUrl)
{
    // Name of custom emoji (e.g. LUL) or name of standard emoji (e.g. slight_smile)
    public string Code => !string.IsNullOrWhiteSpace(Id)
        ? Name
        : EmojiIndex.TryGetCode(Name) ?? Name;
}

public partial record Emoji
{
    private static string GetTwemojiName(string name) => string.Join("-",
        name
            .GetRunes()
            // Variant selector rune is skipped in Twemoji names
            .Where(r => r.Value != 0xfe0f)
            .Select(r => r.Value.ToString("x"))
    );

    private static string GetImageUrl(string id, bool isAnimated) => isAnimated
        ? $"https://cdn.discordapp.com/emojis/{id}.gif"
        : $"https://cdn.discordapp.com/emojis/{id}.png";

    private static string GetImageUrl(string name) =>
        $"https://twemoji.maxcdn.com/2/svg/{GetTwemojiName(name)}.svg";

    public static string GetImageUrl(string? id, string? name, bool isAnimated)
    {
        // Custom emoji
        if (!string.IsNullOrWhiteSpace(id))
            return GetImageUrl(id, isAnimated);

        // Standard emoji
        if (!string.IsNullOrWhiteSpace(name))
            return GetImageUrl(name);

        // Either ID or name should be set
        throw new ApplicationException("Emoji has neither ID nor name set.");
    }

    public static Emoji Parse(JsonElement json)
    {
        var id = json.GetPropertyOrNull("id")?.GetNonWhiteSpaceStringOrNull();
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