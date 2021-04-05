using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    public partial class ChannelCategory : IHasId, IHasPosition
    {
        public Snowflake Id { get; }

        public string Name { get; }

        public int? Position { get; }

        public ChannelCategory(Snowflake id, string name, int? position)
        {
            Id = id;
            Name = name;
            Position = position;
        }

        public override string ToString() => Name;

    }

    public partial class ChannelCategory
    {
        public static ChannelCategory Parse(JsonElement json, int? position = null)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            position ??= json.GetPropertyOrNull("position")?.GetInt32();

            var name = json.GetPropertyOrNull("name")?.GetString() ??
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                id.ToString();

            return new ChannelCategory(
                id,
                name,
                position
            );
        }

        public static ChannelCategory Empty { get; } = new(Snowflake.Zero, "<unknown category>", 0);
    }
}
