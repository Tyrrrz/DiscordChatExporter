namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class FileSizePartitionLimit(long limit) : PartitionLimit
{
    public override bool IsReached(long messagesWritten, long bytesWritten) =>
        bytesWritten >= limit;
}
