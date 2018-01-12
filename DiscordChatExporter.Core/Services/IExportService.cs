using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IExportService
    {
        Task ExportAsync(ExportFormat format, string filePath, ChannelChatLog log);
    }
}