namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-image-structure

    public class EmbedImage
    {
        public string Url { get; }

        public int? Height { get; }

        public int? Width { get; }

        public EmbedImage(string url, int? height, int? width)
        {
            Url = url;
            Height = height;
            Width = width;
        }
    }
}