using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IExportService
    {
        void Export(string filePath, ChannelChatLog channelChatLog, Theme theme);
    }
}