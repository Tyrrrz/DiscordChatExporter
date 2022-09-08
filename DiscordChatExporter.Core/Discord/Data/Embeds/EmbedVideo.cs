using JsonExtensions.Reading;
using System.Text.Json;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/channel#embed-object-embed-video-structure
public record EmbedVideo(
    string? Url,
    int? Width,
    int? Height,
    string? ProxyUrl)
{
    public static EmbedVideo Parse(JsonElement json)
    {
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
        var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetNonWhiteSpaceStringOrNull();

        return new EmbedVideo(url, width, height, proxyUrl);
    }
}