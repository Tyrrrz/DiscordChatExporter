using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Gui.Services
{
    public class SettingsService : SettingsManager
    {
        public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsDarkModeEnabled { get; set; }

        public bool IsTokenPersisted { get; set; } = true;

        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";

        public int ParallelLimit { get; set; } = 1;

        public bool ShouldReuseMedia { get; set; }

        public AuthToken? LastToken { get; set; }

        public ExportFormat LastExportFormat { get; set; } = ExportFormat.HtmlDark;

        public int? LastPartitionLimit { get; set; }

        public bool LastShouldDownloadMedia { get; set; }

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }

        public bool ShouldSerializeLastToken() => IsTokenPersisted;
    }
}