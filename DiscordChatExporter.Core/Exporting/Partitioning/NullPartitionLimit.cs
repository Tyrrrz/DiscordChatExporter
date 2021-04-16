namespace DiscordChatExporter.Core.Exporting.Partitioning
{
    public class NullPartitionLimit : PartitionLimit
    {
        public static NullPartitionLimit Instance { get; } = new();

        public override bool IsReached(long messagesWritten, long bytesWritten) => false;
    }
}