namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#reaction-object

    public class Reaction
    {
        public int Count { get; }

        public Emoji Emoji { get; }

        public Reaction(int count, Emoji emoji)
        {
            Count = count;
            Emoji = emoji;
        }
    }
}