using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface ISettingsService
    {
        string DateFormat { get; set; }
        int MessageGroupLimit { get; set; }

        string LastToken { get; set; }
        ExportFormat LastExportFormat { get; set; }

        void Load();
        void Save();
    }
}