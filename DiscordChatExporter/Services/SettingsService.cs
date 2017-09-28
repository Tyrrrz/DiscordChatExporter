using DiscordChatExporter.Models;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Services
{
    public class SettingsService : SettingsManager, ISettingsService
    {
        public string Token { get; set; }
        public Theme Theme { get; set; }
        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm";
        public int MessageGroupLimit { get; set; } = 20;

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}