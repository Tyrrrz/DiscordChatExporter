namespace DiscordChatExporter.Core.Exporting.MessagesLimitation;

internal class MessagesCountLimit : MessagesLimit
{
    private readonly int _limit;

    public MessagesCountLimit(int limit) => _limit = limit;

    public override bool IsReached(int messagesCount) =>
        messagesCount >= _limit;
}