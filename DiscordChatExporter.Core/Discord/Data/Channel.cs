using System.Diagnostics.CodeAnalysis;
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

        public ChannelKind Kind { get; }

        public bool IsTextChannel => Kind is
            ChannelKind.GuildTextChat or
            ChannelKind.DirectTextChat or
            ChannelKind.DirectGroupTextChat or
            ChannelKind.GuildNews or
            ChannelKind.GuildStore;

        public bool IsVoiceChannel => !IsTextChannel;

        public Snowflake GuildId { get; }

        public ChannelCategory Category { get; }

        public string Name { get; }

        public int? Position { get; }

        public string? Topic { get; }

        public Channel(
            Snowflake id,
            ChannelKind kind,
            Snowflake guildId,
            ChannelCategory category,
            string name,
            int? position,
            string? topic)
        {
            Id = id;
            Kind = kind;
            GuildId = guildId;
            Category = category;
            Name = name;
            Position = position;
            Topic = topic;
        }
        public Channel(
            Snowflake id,
            ChannelKind kind,
            Snowflake guildId,
            string name,
            string? topic)
        {
            Id = id;
            Kind = kind;
            GuildId = guildId;
            Name = name;
            Topic = topic;
            Category = ChannelCategory.Unknown;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => Name;
    }

    public partial class Channel
    {
        private static ChannelCategory GetFallbackCategory(ChannelKind channelKind) => new(
            Snowflake.Zero,
            channelKind switch
            {
                ChannelKind.GuildTextChat => "Text",
                ChannelKind.DirectTextChat => "Private",
                ChannelKind.DirectGroupTextChat => "Group",
                ChannelKind.GuildNews => "News",
                ChannelKind.GuildStore => "Store",
                _ => "Default"
            },
            null
        );

        public static Channel Parse(JsonElement json, ChannelCategory? category = null, int? position = null)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString().Pipe(Snowflake.Parse);
            var topic = json.GetPropertyOrNull("topic")?.GetString();
            var kind = (ChannelKind) json.GetProperty("type").GetInt32();

            var name =
                // Guild channel
                json.GetPropertyOrNull("name")?.GetString() ??
                // DM channel
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                // Fallback
                id.ToString();

            return new Channel(
                id,
                kind,
                guildId ?? Guild.DirectMessages.Id,
                category ?? GetFallbackCategory(kind),
                name,
                position ?? json.GetPropertyOrNull("position")?.GetInt32(),
                topic
            );
        }
        public static Channel Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString().Pipe(Snowflake.Parse);
            var topic = json.GetPropertyOrNull("topic")?.GetString();
            var kind = (ChannelKind)json.GetProperty("type").GetInt32();

            var name =
                 json.GetPropertyOrNull("name")?.GetString() ??
                 json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                 id.ToString();

            return new Channel(
                id,
                kind,
                guildId ?? Guild.DirectMessages.Id,
                name,
                topic
            );
        }
    }
}