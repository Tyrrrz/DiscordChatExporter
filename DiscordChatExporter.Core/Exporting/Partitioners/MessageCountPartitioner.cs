using DiscordChatExporter.Core.Exporting.Partitioners;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    public class MessageCountPartitioner : IPartitioner
    {

        private int _messagesPerPartition;

        public MessageCountPartitioner(int messagesPerPartition)
        {
            _messagesPerPartition = messagesPerPartition;
        }

        public bool IsLimitReached(ExportPartitioningContext context)
        {
            return context.MessageCount > 0 &&
            _messagesPerPartition != 0 &&
            context.MessageCount % _messagesPerPartition == 0;
        }
    }
}
