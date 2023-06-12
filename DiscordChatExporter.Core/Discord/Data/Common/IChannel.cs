namespace DiscordChatExporter.Core.Discord.Data.Common;

public interface IChannel : IHasId
{
    ChannelKind Kind { get; }
    Snowflake GuildId { get; }
    string Name { get; }
    Snowflake? LastMessageId { get; }
}
