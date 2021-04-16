using DiscordChatExporter.Core.Discord.Data.Common;

namespace DiscordChatExporter.Core.Exporting.Partitioning
{
    public abstract partial class PartitionLimit
    {
        public abstract bool IsReached(long messagesWritten, long bytesWritten);
    }

    public partial class PartitionLimit
    {
        public static PartitionLimit Parse(string value)
        {
            var fileSize = FileSize.TryParse(value);
            if (fileSize is not null)
                return new FileSizePartitionLimit(fileSize.Value.TotalBytes);

            var messageCount = int.Parse(value);
            return new MessageCountPartitionLimit(messageCount);
        }
    }
}