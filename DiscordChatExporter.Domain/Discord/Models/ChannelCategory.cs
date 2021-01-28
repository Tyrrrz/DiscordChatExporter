using System;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Utilities;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    public partial class ChannelCategory : IHasId
    {
        public Snowflake Id { get; }

        public string Name { get; }

        public int Position { get; }

        public ChannelCategory(Snowflake id, string name, int position)
        {
            Id = id;
            Name = name;
            Position = position;
        }

        public override string ToString() => Name;

    }

    public partial class ChannelCategory
    {
        public static ChannelCategory Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var position = json.GetProperty("position").GetInt32();

            var name = json.GetPropertyOrNull("name")?.GetString() ??
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                id.ToString();

            return new ChannelCategory(
                id,
                name,
                position
            );
        }
    }
}