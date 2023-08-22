namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class MessageCountPartitionLimit : PartitionLimit
{
    private readonly long _limit;

    public MessageCountPartitionLimit(long limit) => _limit = limit;

    public override bool IsReached(long messagesWritten, long bytesWritten) =>
        messagesWritten >= _limit;
}
