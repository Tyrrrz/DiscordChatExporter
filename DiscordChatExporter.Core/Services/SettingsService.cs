using DiscordChatExporter.Core.Models;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Core.Services
{
    public class SettingsService : SettingsManager, ISettingsService
    {
        public bool IsAutoUpdateEnabled { get; set; } = true;

        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";
        public int MessageGroupLimit { get; set; } = 20;

        public AuthToken LastToken { get; set; }
        public ExportFormat LastExportFormat { get; set; } = ExportFormat.HtmlDark;
        public int? LastPartitionLimit { get; set; }

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}