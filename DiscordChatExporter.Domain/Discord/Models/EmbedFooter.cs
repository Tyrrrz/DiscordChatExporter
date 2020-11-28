using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
    public partial class EmbedFooter
    {
        public string Text { get; }

        public string? IconUrl { get; }

        public EmbedFooter(string text, string? iconUrl)
        {
            Text = text;
            IconUrl = iconUrl;
        }

        public override string ToString() => Text;
    }

    public partial class EmbedFooter
    {
        public static EmbedFooter Parse(JsonElement json)
        {
            var text = json.GetProperty("text").GetString();
            var iconUrl = json.GetPropertyOrNull("icon_url")?.GetString();

            return new EmbedFooter(text, iconUrl);
        }
    }
}