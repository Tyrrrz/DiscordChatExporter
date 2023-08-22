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

public static class ChannelKindExtensions
{
    public static bool IsDirect(this ChannelKind kind) =>
        kind is ChannelKind.DirectTextChat or ChannelKind.DirectGroupTextChat;

    public static bool IsGuild(this ChannelKind kind) => !kind.IsDirect();

    public static bool IsVoice(this ChannelKind kind) =>
        kind is ChannelKind.GuildVoiceChat or ChannelKind.GuildStageVoice;

    public static bool IsThread(this ChannelKind kind) =>
        kind
            is ChannelKind.GuildNewsThread
                or ChannelKind.GuildPublicThread
                or ChannelKind.GuildPrivateThread;
}
