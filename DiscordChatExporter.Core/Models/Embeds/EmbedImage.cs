namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-image-structure

    public class EmbedImage
    {
        public string Url { get; }

        public string ProxyUrl { get; }

        public int? Height { get; }

        public int? Width { get; }

        public EmbedImage(string url, string proxyUrl, int? height, int? width)
        {
            Url = url;
            ProxyUrl = proxyUrl;
            Height = height;
            Width = width;
        }
    }
}