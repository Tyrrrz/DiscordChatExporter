using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.ViewModels.Framework
{
    // Used to instantiate new view models while making use of dependency injection
    public interface IViewModelFactory
    {
        ChannelViewModel CreateChannelViewModel();

        GuildViewModel CreateGuildViewModel();

        ExportSetupViewModel CreateExportSetupViewModel();

        SettingsViewModel CreateSettingsViewModel();
    }
}