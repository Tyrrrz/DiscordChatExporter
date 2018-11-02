using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IExportService
    {
        void ExportChatLog(ChatLog chatLog, string filePath, ExportFormat format);

        void ExportChatLog(ChatLog chatLog, string filePath, ExportFormat format, int maxMessageCountPerPartition);
    }
}