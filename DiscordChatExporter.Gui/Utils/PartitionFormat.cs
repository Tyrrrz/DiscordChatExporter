using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Gui.Internal
{
    public enum PartitionFormat
    {
        MessageCount,
        Filesize,
    }

    public static class PartitionFormatExtensions
    {
        public static string GetDisplayName(this PartitionFormat format) => format switch
        {
            PartitionFormat.MessageCount => "Message count",
            PartitionFormat.Filesize => "File size (MB)",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }
}
