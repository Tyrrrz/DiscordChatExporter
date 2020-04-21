namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-author-structure

    public class EmbedAuthor
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
}