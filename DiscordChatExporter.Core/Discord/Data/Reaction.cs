using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#reaction-object
    public partial class Reaction
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

    public partial class Reaction
    {
        public static Reaction Parse(JsonElement json)
        {
            var count = json.GetProperty("count").GetInt32();
            var emoji = json.GetProperty("emoji").Pipe(Emoji.Parse);

            return new Reaction(emoji, count);
        }
    }
}