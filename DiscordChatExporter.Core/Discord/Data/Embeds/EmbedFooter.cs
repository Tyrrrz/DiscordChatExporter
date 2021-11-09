using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
    public record EmbedFooter(
        string Text,
        string? IconUrl,
        string? IconProxyUrl)
    {
        public static EmbedFooter Parse(JsonElement json)
        {
            var text = json.GetProperty("text").GetNonWhiteSpaceString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetStringOrNull();
            var iconProxyUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetStringOrNull();

            return new EmbedFooter(text, iconUrl, iconProxyUrl);
        }
    }
}