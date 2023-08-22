namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class FileSizePartitionLimit : PartitionLimit
{
    private readonly long _limit;

    public FileSizePartitionLimit(long limit) => _limit = limit;

    public override bool IsReached(long messagesWritten, long bytesWritten) =>
        bytesWritten >= _limit;
}
