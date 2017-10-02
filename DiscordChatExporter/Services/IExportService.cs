using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IExportService
    {
        Task ExportAsync(ExportFormat format, string filePath, ChannelChatLog log);
    }
}