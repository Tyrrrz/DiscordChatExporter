namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public interface IHasId
    {
        Snowflake Id { get; }
    }
}