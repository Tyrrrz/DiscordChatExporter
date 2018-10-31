using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IExportService
    {
        void Export(ChatLog chatLog, string filePath, ExportFormat format);
    }
}