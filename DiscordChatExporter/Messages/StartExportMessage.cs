using DiscordChatExporter.Models;

namespace DiscordChatExporter.Messages
{
    public class StartExportMessage
    {
        public Channel Channel { get; }

        public string FilePath { get; }

        public ExportFormat Format { get; }

        public StartExportMessage(Channel channel, string filePath, ExportFormat format)
        {
            Channel = channel;
            FilePath = filePath;
            Format = format;
        }
    }
}