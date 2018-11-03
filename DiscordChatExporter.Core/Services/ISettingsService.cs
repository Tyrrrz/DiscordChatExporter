using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface ISettingsService
    {
        bool IsAutoUpdateEnabled { get; set; }

        string DateFormat { get; set; }
        int MessageGroupLimit { get; set; }

        AuthToken LastToken { get; set; }
        ExportFormat LastExportFormat { get; set; }
        int? LastMaxMessageCountPerPartition { get; set; }

        void Load();
        void Save();
    }
}