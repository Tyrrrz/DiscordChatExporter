namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-provider-structure

    public class EmbedProvider
    {
        public string Name { get; }

        public string Url { get; }

        public EmbedProvider(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}