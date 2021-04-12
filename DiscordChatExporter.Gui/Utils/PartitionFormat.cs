using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Gui.Internal
{
    public enum PartitionFormat
    {
        MessageCount,
        FileSize,
    }

    public static class PartitionFormatExtensions
    {
        public static string GetDisplayName(this PartitionFormat format) => format switch
        {
            PartitionFormat.MessageCount => "Message count",
            PartitionFormat.FileSize => "File size (MB)",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }
}
