using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IExportService
    {
        Task ExportAsTextAsync(string filePath, ChannelChatLog log);
        Task ExportAsHtmlAsync(string filePath, ChannelChatLog log, Theme theme);
    }
}