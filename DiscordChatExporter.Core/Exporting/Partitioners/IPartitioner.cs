using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    interface IPartitioner
    {
        bool IsLimitReached(long messageCount, long sizeInBytes);
    }
}
