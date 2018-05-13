namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-author-structure

    public class EmbedAuthor
    {
        public string Name { get; }

        public string Url { get; }

        public string IconUrl { get; }

        public string ProxyIconUrl { get; }

        public EmbedAuthor(string name, string url, string iconUrl, string proxyIconUrl)
        {
            Name = name;
            Url = url;
            IconUrl = iconUrl;
            ProxyIconUrl = proxyIconUrl;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}