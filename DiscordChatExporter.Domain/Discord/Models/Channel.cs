using System;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Utilities;
using JsonExtensions.Reading;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/channel#channel-object-channel-types
    // Order of enum fields needs to match the order in the docs.
    public enum ChannelType
    {
        GuildTextChat,
        DirectTextChat,
        GuildVoiceChat,
        DirectGroupTextChat,
        GuildCategory,
        GuildNews,
        GuildStore
    }

    // https://discord.com/developers/docs/resources/channel#channel-object
    public partial class Channel : IHasId, IComparable, IComparable<Channel>
    {
        public Snowflake Id { get; }

        public ChannelType Type { get; }

        public bool IsTextChannel =>
            Type == ChannelType.GuildTextChat ||
            Type == ChannelType.DirectTextChat ||
            Type == ChannelType.DirectGroupTextChat ||
            Type == ChannelType.GuildNews ||
            Type == ChannelType.GuildStore;

        public Snowflake GuildId { get; }

        public Channel Category { get; }

        public string Name { get; }

        public int Position { get; }

        public string? Topic { get; }

        public Channel(Snowflake id, ChannelType type, Snowflake guildId, Channel? category, string name, int position, string? topic)
        {
            Id = id;
            Type = type;
            GuildId = guildId;
            Category = category ?? (type != ChannelType.GuildCategory ? GetDefaultCategory(type, guildId) : this);
            Name = name;
            Position = position;
            Topic = topic;
        }

        public int CompareTo(Channel? other)
        {
            return other != null ? Position.CompareTo(other.Position) : 1;
        }

        public int CompareTo(object? obj)
        {
            return obj is Channel other ? CompareTo(other) : 1;
        }

        public override string ToString() => Name;

    }

    public partial class Channel
    {
        private static Channel GetDefaultCategory(ChannelType channelType, Snowflake guildId) => new(
                Snowflake.Zero,
                ChannelType.GuildCategory,
                guildId,
                null,
                channelType switch
                {
                    ChannelType.GuildTextChat => "Text",
                    ChannelType.DirectTextChat => "Private",
                    ChannelType.DirectGroupTextChat => "Group",
                    ChannelType.GuildNews => "News",
                    ChannelType.GuildStore => "Store",
                    _ => "Default"
                },
                -1,
                null
            );

        public static Channel Parse(JsonElement json, Channel? category = null)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString().Pipe(Snowflake.Parse);
            var position = json.GetProperty("position").GetInt32();
            var topic = json.GetPropertyOrNull("topic")?.GetString();

            var type = (ChannelType)json.GetProperty("type").GetInt32();

            var name =
                json.GetPropertyOrNull("name")?.GetString() ??
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                id.ToString();

            return new Channel(
                id,
                type,
                guildId ?? Guild.DirectMessages.Id,
                category ?? GetDefaultCategory(type, guildId ?? Guild.DirectMessages.Id),
                name,
                position,
                topic
            );
        }
    }
}