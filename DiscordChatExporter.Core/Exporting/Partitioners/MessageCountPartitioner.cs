using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting
{
    class MessageCountPartitioner : IPartitioner
    {

        private int _messagesPerPartition;

        public MessageCountPartitioner(int messagesPerPartition)
        {
            _messagesPerPartition = messagesPerPartition;
        }

        public bool IsLimitReached(long messageCount, long sizeInBytes)
        {
            return messageCount > 0 &&
            _messagesPerPartition != 0 &&
            messageCount % _messagesPerPartition == 0;
        }
    }
}
