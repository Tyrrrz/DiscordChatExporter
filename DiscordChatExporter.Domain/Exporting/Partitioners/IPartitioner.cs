using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Domain.Exporting
{
    interface IPartitioner
    {
        bool IsLimitReached(long messageCount, long sizeInBytes);
    }
}
