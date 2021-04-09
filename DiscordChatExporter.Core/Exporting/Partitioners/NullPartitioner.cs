using DiscordChatExporter.Core.Exporting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core
{
    class NullPartitioner : IPartitioner
    {
        public bool IsLimitReached(long messageCount, long sizeInBytes)
        {
            return false;
        }
    }
}
