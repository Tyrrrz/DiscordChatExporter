using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
    public record EmbedField(
        string Name,
        string Value,
        bool IsInline)
    {
        public static EmbedField Parse(JsonElement json)
        {
            var name = json.GetProperty("name").GetNonWhiteSpaceString();
            var value = json.GetProperty("value").GetNonWhiteSpaceString();
            var isInline = json.GetPropertyOrNull("inline")?.GetBoolean() ?? false;

            return new EmbedField(name, value, isInline);
        }
    }
}