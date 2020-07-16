using System.Text.Json;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-author-structure
    public partial class EmbedAuthor
    {
        public string? Name { get; }

        public string? Url { get; }

        public string? IconUrl { get; }

        public EmbedAuthor(string? name, string? url, string? iconUrl)
        {
            Name = name;
            Url = url;
            IconUrl = iconUrl;
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

            return new EmbedAuthor(name, url, iconUrl);
        }
    }
}