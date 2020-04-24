using System.Text.Json;
using DiscordChatExporter.Domain.Internal;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-image-structure
    public partial class EmbedImage
    {
        public string? Url { get; }

        public int? Width { get; }

        public int? Height { get; }

        public EmbedImage(string? url, int? width, int? height)
        {
            Url = url;
            Height = height;
            Width = width;
        }
    }

    public partial class EmbedImage
    {
        public static EmbedImage Parse(JsonElement json)
        {
            var url = json.GetPropertyOrNull("url")?.GetString();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();

            return new EmbedImage(url, width, height);
        }
    }
}