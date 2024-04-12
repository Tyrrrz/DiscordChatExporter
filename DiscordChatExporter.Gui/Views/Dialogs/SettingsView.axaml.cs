using System.Windows;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.Views.Dialogs;

public partial class SettingsView : UserControl<SettingsViewModel>
{
    public SettingsView() => InitializeComponent();

    private void DarkModeToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (DarkModeToggleButton.IsChecked is true)
        {
            App.SetDarkTheme();
        }
        else
        {
            App.SetLightTheme();
        }
    }
}
