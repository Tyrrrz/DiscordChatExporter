using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    class FileSizePartitioner : IPartitioner
    {
        private long _bytesPerFile;

        public FileSizePartitioner(long bytesPerFile)
        {
            _bytesPerFile = bytesPerFile;
        }
        public bool IsLimitReached(long messageCount, long sizeInBytes)
        {
            return sizeInBytes >= _bytesPerFile;
        }
    }
}
