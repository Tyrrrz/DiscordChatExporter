namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-video-structure

    public class EmbedVideo
    {
        public string Url { get; }

        public int? Height { get; }

        public int? Width { get; }

        public EmbedVideo(string url, int? height, int? width)
        {
            Url = url;
            Height = height;
            Width = width;
        }
    }
}