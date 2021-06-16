using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    public partial class ChannelCategory : IHasId
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

            var name =
                json.GetPropertyOrNull("name")?.GetString() ??
                id.ToString();

            return new ChannelCategory(
                id,
                name,
                position ?? json.GetPropertyOrNull("position")?.GetInt32()
            );
        }

        public static ChannelCategory Empty { get; } = new(Snowflake.Zero, "<unknown category>", 0);
    }
}