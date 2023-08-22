using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object-embed-image-structure
public record EmbedImage(string? Url, string? ProxyUrl, int? Width, int? Height)
{
    public static EmbedImage Parse(JsonElement json)
    {
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();

        return new EmbedImage(url, proxyUrl, width, height);
    }
}
