using DiscordChatExporter.Domain.Discord.Models.Common;

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

        public string GuildId { get; }

        public string? ParentId { get; }

        public ChannelType Type { get; }

        public bool IsTextChannel =>
            Type == ChannelType.GuildTextChat ||
            Type == ChannelType.DirectTextChat ||
            Type == ChannelType.DirectGroupTextChat ||
            Type == ChannelType.GuildNews ||
            Type == ChannelType.GuildStore;

        public string Name { get; }

        public string? Topic { get; }

        public Channel(string id, string guildId, string? parentId, ChannelType type, string name, string? topic)
        {
            Id = id;
            GuildId = guildId;
            ParentId = parentId;
            Type = type;
            Name = name;
            Topic = topic;
        }

        public override string ToString() => Name;
    }

    public partial class Channel
    {
        public static Channel CreateDeletedChannel(string id) =>
            new Channel(id, "unknown-guild", null, ChannelType.GuildTextChat, "deleted-channel", null);
    }
}