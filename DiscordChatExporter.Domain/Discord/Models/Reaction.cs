namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#reaction-object

    public class Reaction
    {
        public Emoji Emoji { get; }

        public int Count { get; }

        public Reaction(Emoji emoji, int count)
        {
            Emoji = emoji;
            Count = count;
        }

        public override string ToString() => $"{Emoji} ({Count})";
    }
}