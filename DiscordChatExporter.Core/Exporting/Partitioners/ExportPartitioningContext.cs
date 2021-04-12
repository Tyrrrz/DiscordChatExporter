using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting.Partitioners
{
    public class ExportPartitioningContext
    {
        public long MessageCount { get; } 
        public long SizeInBytes { get; }

        public ExportPartitioningContext(long messageCount, long sizeInBytes)
        {
            MessageCount = messageCount;
            SizeInBytes = sizeInBytes;
        }
    }
}
