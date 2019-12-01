namespace DiscordChatExporter.Core.Models
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
}