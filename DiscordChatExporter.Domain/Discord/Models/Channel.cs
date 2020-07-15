using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#channel-object-channel-types
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

    // https://discordapp.com/developers/docs/resources/channel#channel-object
    public partial class Channel : IHasId
    {
        public string Id { get; }

        public ChannelType Type { get; }

        public bool IsTextChannel =>
            Type == ChannelType.GuildTextChat ||
            Type == ChannelType.DirectTextChat ||
            Type == ChannelType.DirectGroupTextChat ||
            Type == ChannelType.GuildNews ||
            Type == ChannelType.GuildStore;

        public string GuildId { get; }

        public string Category { get; }

        public string Name { get; }

        public string? Topic { get; }

        public Channel(string id, ChannelType type, string guildId, string category, string name, string? topic)
        {
            Id = id;
            Type = type;
            GuildId = guildId;
            Category = category;
            Name = name;
            Topic = topic;
        }

        public override string ToString() => Name;
    }

    public partial class Channel
    {
        private static string GetDefaultCategory(ChannelType channelType) => channelType switch
        {
            ChannelType.GuildTextChat => "Text",
            ChannelType.DirectTextChat => "Private",
            ChannelType.DirectGroupTextChat => "Group",
            ChannelType.GuildNews => "News",
            ChannelType.GuildStore => "Store",
            _ => "Default"
        };

        public static Channel Parse(JsonElement json, string? category = null)
        {
            var id = json.GetProperty("id").GetString();
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString();
            var topic = json.GetPropertyOrNull("topic")?.GetString();

            var type = (ChannelType) json.GetProperty("type").GetInt32();

            var name =
                json.GetPropertyOrNull("name")?.GetString() ??
                json.GetPropertyOrNull("recipients")?.EnumerateArray().Select(User.Parse).Select(u => u.Name).JoinToString(", ") ??
                id;

            return new Channel(
                id,
                type,
                guildId ?? Guild.DirectMessages.Id,
                category ?? GetDefaultCategory(type),
                name,
                topic);
        }
    }
}