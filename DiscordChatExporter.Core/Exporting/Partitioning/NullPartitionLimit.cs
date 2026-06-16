namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class NullPartitionLimit : PartitionLimit
{
    public override bool IsReached(long messagesWritten, long bytesWritten) => false;
}
