using System;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Gui.Messages
{
    public class StartExportMessage
    {
        public Channel Channel { get; }

        public string FilePath { get; }

        public ExportFormat Format { get; }

        public DateTime? From { get; }

        public DateTime? To { get; }

        public StartExportMessage(Channel channel, string filePath, ExportFormat format,
            DateTime? from, DateTime? to)
        {
            Channel = channel;
            FilePath = filePath;
            Format = format;
            From = from;
            To = to;
        }
    }
}