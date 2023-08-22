using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
public record EmbedField(string Name, string Value, bool IsInline)
{
    public static EmbedField Parse(JsonElement json)
    {
        var name = json.GetProperty("name").GetNonNullString();
        var value = json.GetProperty("value").GetNonNullString();
        var isInline = json.GetPropertyOrNull("inline")?.GetBooleanOrNull() ?? false;

        return new EmbedField(name, value, isInline);
    }
}
