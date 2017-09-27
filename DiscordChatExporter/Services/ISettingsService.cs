using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface ISettingsService
    {
        string Token { get; set; }
        Theme Theme { get; set; }

        void Load();
        void Save();
    }
}