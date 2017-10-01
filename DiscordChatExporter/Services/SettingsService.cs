using DiscordChatExporter.Models;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Services
{
    public class SettingsService : SettingsManager, ISettingsService
    {
        public Theme Theme { get; set; }
        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";
        public int MessageGroupLimit { get; set; } = 20;

        public string LastToken { get; set; }
        public ExportFormat LastExportFormat { get; set; } = ExportFormat.Html;

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}