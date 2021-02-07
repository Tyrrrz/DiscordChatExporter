using DiscordChatExporter.Domain.Exporting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Domain
{
    class NullPartitioner : IPartitioner
    {
        public bool IsLimitReached(long messageCount, long sizeInBytes)
        {
            return false;
        }
    }
}
