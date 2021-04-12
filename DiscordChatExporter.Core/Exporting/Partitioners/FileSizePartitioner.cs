using DiscordChatExporter.Core.Exporting.Partitioners;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    public class FileSizePartitioner : IPartitioner
    {
        private long _bytesPerFile;

        public FileSizePartitioner(long bytesPerFile)
        {
            _bytesPerFile = bytesPerFile;
        }
        public bool IsLimitReached(ExportPartitioningContext context)
        {
            return context.SizeInBytes >= _bytesPerFile;
        }
    }
}
