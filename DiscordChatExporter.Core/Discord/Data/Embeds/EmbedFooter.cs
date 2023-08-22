using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
public record EmbedFooter(string Text, string? IconUrl, string? IconProxyUrl)
{
    public static EmbedFooter Parse(JsonElement json)
    {
        var text = json.GetProperty("text").GetNonNullString();
        var iconUrl = json.GetPropertyOrNull("icon_url")?.GetNonWhiteSpaceStringOrNull();
        var iconProxyUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetNonWhiteSpaceStringOrNull();

        return new EmbedFooter(text, iconUrl, iconProxyUrl);
    }
}
