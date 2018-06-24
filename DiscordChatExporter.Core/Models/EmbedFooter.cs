namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-footer-structure

    public class EmbedFooter
    {
        public string Text { get; }

        public string IconUrl { get; }

        public EmbedFooter(string text, string iconUrl)
        {
            Text = text;
            IconUrl = iconUrl;
        }

        public override string ToString() => Text;
    }
}