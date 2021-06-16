using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#channel-object
    public partial class Channel : IHasId
    {
        public Snowflake Id { get; }

        public ChannelType Type { get; }

        public bool IsTextChannel => Type is
            ChannelType.GuildTextChat or
            ChannelType.DirectTextChat or
            ChannelType.DirectGroupTextChat or
            ChannelType.GuildNews or
            ChannelType.GuildStore;

        public Snowflake GuildId { get; }

        public ChannelCategory Category { get; }

        public string Name { get; }

        public int? Position { get; }

        public string? Topic { get; }

        public Channel(
            Snowflake id,
            ChannelType type,
            Snowflake guildId,
            ChannelCategory category,
            string name,
            int? position,
            string? topic)
        {
            Id = id;
            Type = type;
            GuildId = guildId;
            Category = category;
            Name = name;
            Position = position;
            Topic = topic;
        }

        public override string ToString() => Name;
    }

    public partial class Channel
    {
        private static ChannelCategory GetFallbackCategory(ChannelType channelType) => new(
            Snowflake.Zero,
            channelType switch
            {
                ChannelType.GuildTextChat => "Text",
                ChannelType.DirectTextChat => "Private",
                ChannelType.DirectGroupTextChat => "Group",
                ChannelType.GuildNews => "News",
                ChannelType.GuildStore => "Store",
                _ => "Default"
            },
            null
        );

        public static Channel Parse(JsonElement json, ChannelCategory? category = null, int? position = null)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString().Pipe(Snowflake.Parse);
            var topic = json.GetPropertyOrNull("topic")?.GetString();
            var type = (ChannelType) json.GetProperty("type").GetInt32();

            var name =
                // Guild channel
                json.GetPropertyOrNull("name")?.GetString() ??
                // DM channel
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                // Fallback
                id.ToString();

            return new Channel(
                id,
                type,
                guildId ?? Guild.DirectMessages.Id,
                category ?? GetFallbackCategory(type),
                name,
                position ?? json.GetPropertyOrNull("position")?.GetInt32(),
                topic
            );
        }
    }
}