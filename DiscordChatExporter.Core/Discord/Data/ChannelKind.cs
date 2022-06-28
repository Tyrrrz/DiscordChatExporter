namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#channel-object-channel-types
// Order of enum fields needs to match the order in the docs.
public enum ChannelKind
{
    GuildTextChat = 0,
    DirectTextChat,
    GuildVoiceChat,
    DirectGroupTextChat,
    GuildCategory,
    GuildNews,
    GuildStore
}

public static class ChannelKindExtensions
{
    public static bool IsText(this ChannelKind kind) => kind is
        ChannelKind.GuildTextChat or
        ChannelKind.DirectTextChat or
        ChannelKind.DirectGroupTextChat or
        ChannelKind.GuildNews or
        ChannelKind.GuildStore;

    public static bool IsVoice(this ChannelKind kind) => kind is
        ChannelKind.GuildVoiceChat;
}