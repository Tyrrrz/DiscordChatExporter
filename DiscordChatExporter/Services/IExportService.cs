using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IExportService
    {
        Task ExportAsync(string filePath, ChannelChatLog log, Theme theme);
    }
}