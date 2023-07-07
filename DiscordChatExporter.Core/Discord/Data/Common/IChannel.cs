namespace DiscordChatExporter.Core.Discord.Data.Common;

public interface IChannel : IHasId
{
    ChannelKind Kind { get; }
    Snowflake GuildId { get; }
    Snowflake ParentId { get; }
    string? ParentName { get; }
    int? ParentPosition { get; }
    string Name { get; }
    int? Position { get; }
    string? IconUrl { get; }
    string? Topic { get; }
    Snowflake? LastMessageId { get; }
}
