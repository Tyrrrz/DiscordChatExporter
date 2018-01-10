using System;
using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.ViewModels
{
    public interface ICliViewModel
    {
        Task ExportAsync(string token, string channelId, string filePath, ExportFormat format, DateTime? from,
            DateTime? to);
    }
}