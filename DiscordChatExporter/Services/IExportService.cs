using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IExportService
    {
        void Export(string filePath, ChatLog chatLog, Theme theme);
    }
}