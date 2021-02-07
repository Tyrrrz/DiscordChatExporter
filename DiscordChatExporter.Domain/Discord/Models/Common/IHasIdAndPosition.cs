namespace DiscordChatExporter.Domain.Discord.Models.Common
{
    public interface IHasIdAndPosition : IHasId
    {
        int? Position { get; }
    }
}