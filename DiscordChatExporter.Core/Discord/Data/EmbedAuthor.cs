using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-author-structure
    public partial class EmbedAuthor
    {
        public string? Name { get; }

        public string? Url { get; }

        public string? IconUrl { get; }

        public string? IconProxyUrl { get; }

        public EmbedAuthor(string? name, string? url, string? iconUrl, string? iconProxyUrl)
        {
            Name = name;
            Url = url;
            IconUrl = iconUrl;
            IconProxyUrl = iconProxyUrl;
        }

        public override string ToString() => Name ?? "<unnamed author>";
    }

    public partial class EmbedAuthor
    {
        public static EmbedAuthor Parse(JsonElement json)
        {
            var name = json.GetPropertyOrNull("name")?.GetString();
            var url = json.GetPropertyOrNull("url")?.GetString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetString();
            var iconProxyUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetString();

            return new EmbedAuthor(name, url, iconUrl, iconProxyUrl);
        }
    }
}