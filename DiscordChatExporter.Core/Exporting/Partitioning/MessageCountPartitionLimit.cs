namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class MessageCountPartitionLimit(long limit) : PartitionLimit
{
    public override bool IsReached(long messagesWritten, long bytesWritten) =>
        messagesWritten >= limit;
}
