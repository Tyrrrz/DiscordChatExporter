using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Partitioners;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    public class NullPartitioner : IPartitioner
    {
        public bool IsLimitReached(ExportPartitioningContext context)
        {
            return false;
        }
    }
}
