namespace DiscordChatExporter.Gui.ViewModels
{
    public interface ISettingsViewModel
    {
        string DateFormat { get; set; }
        int MessageGroupLimit { get; set; }
    }
}