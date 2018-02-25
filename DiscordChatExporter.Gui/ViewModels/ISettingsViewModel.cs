namespace DiscordChatExporter.Gui.ViewModels
{
    public interface ISettingsViewModel
    {
        bool IsAutoUpdateEnabled { get; set; }

        string DateFormat { get; set; }
        int MessageGroupLimit { get; set; }
    }
}