using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-image-structure
    public record EmbedImage(
        string? Url,
        string? ProxyUrl,
        int? Width,
        int? Height)
    {
        public static EmbedImage Parse(JsonElement json)
        {
            var url = json.GetPropertyOrNull("url")?.GetStringOrNull();
            var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetStringOrNull();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();

            return new EmbedImage(url, proxyUrl, width, height);
        }
    }
}