namespace DiscordChatExporter.Services
{
    public interface ISettingsService
    {
        string Token { get; set; }

        void Load();
        void Save();
    }
}