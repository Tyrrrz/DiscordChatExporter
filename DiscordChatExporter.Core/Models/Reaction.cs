namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#reaction-object

    public class Reaction
    {
        public int Count { get; }

        public string EmojiId { get; }

        public string EmojiName { get; }

        public Reaction(int count, string emojiId, string emojiName) 
        {
            Count = count;
            EmojiId = emojiId;
            EmojiName = emojiName;
        }
    }
}