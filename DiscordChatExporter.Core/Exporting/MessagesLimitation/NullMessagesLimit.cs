namespace DiscordChatExporter.Core.Exporting.MessagesLimitation;

internal class NullMessagesLimit : MessagesLimit
{
    public override bool IsReached(int messagesCount) => false;
}