namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#channel-object-channel-types
public enum ChannelKind
{
    GuildTextChat = 0,
    DirectTextChat = 1,
    GuildVoiceChat = 2,
    DirectGroupTextChat = 3,
    GuildCategory = 4,
    GuildNews = 5,
    GuildNewsThread = 10,
    GuildPublicThread = 11,
    GuildPrivateThread = 12,
    GuildStageVoice = 13,
    GuildDirectory = 14,
    GuildForum = 15
}
