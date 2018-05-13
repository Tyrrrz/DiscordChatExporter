namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-footer-structure

    public class EmbedFooter
    {
        public string Text { get; }

        public string IconUrl { get; }

        public string ProxyIconUrl { get; }

        public EmbedFooter(string text, string iconUrl, string proxyIconUrl)
        {
            Text = text;
            IconUrl = iconUrl;
            ProxyIconUrl = proxyIconUrl;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}