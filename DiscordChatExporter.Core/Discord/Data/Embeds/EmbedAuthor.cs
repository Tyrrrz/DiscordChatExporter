using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
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

        [ExcludeFromCodeCoverage]
        public override string ToString() => Name ?? "<unnamed author>";
    }

    public partial class EmbedAuthor
    {
        public static EmbedAuthor Parse(JsonElement json)
        {
            var name = json.GetPropertyOrNull("name")?.GetStringOrNull();
            var url = json.GetPropertyOrNull("url")?.GetStringOrNull();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetStringOrNull();
            var iconProxyUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetStringOrNull();

            return new EmbedAuthor(name, url, iconUrl, iconProxyUrl);
        }
    }
}