using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object-embed-video-structure
public record EmbedVideo(string? Url, string? ProxyUrl, int? Width, int? Height)
{
    public static EmbedVideo Parse(JsonElement json)
    {
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();

        return new EmbedVideo(url, proxyUrl, width, height);
    }
}
