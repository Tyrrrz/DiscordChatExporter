namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#channel-object-channel-types

    public enum ChannelType
    {
        GuildTextChat,
        DirectTextChat,
        GuildVoiceChat,
        DirectGroupTextChat,
        Category
    }
}