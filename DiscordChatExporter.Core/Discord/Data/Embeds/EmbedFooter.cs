using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
    public partial class EmbedFooter
    {
        public string Text { get; }

        public string? IconUrl { get; }

        public string? IconProxyUrl { get; }

        public EmbedFooter(string text, string? iconUrl, string? iconProxyUrl)
        {
            Text = text;
            IconUrl = iconUrl;
            IconProxyUrl = iconProxyUrl;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => Text;
    }

    public partial class EmbedFooter
    {
        public static EmbedFooter Parse(JsonElement json)
        {
            var text = json.GetProperty("text").GetString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetString();
            var iconProxyUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetString();

            return new EmbedFooter(text, iconUrl, iconProxyUrl);
        }
    }
}