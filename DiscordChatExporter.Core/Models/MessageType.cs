namespace DiscordChatExporter.Core.Models
{
    public enum MessageType
    {
        Default,
        RecipientAdd,
        RecipientRemove,
        Call,
        ChannelNameChange,
        ChannelIconChange,
        ChannelPinnedMessage,
        GuildMemberJoin
    }
}