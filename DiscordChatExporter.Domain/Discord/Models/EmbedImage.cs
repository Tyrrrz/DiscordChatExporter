namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-image-structure

    public class EmbedImage
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
}