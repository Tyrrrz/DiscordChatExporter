using System.Windows;
using Avalonia.Interactivity;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.Views.Dialogs;

public partial class SettingsView : UserControl<SettingsViewModel>
{
    public SettingsView() => InitializeComponent();

    private void DarkModeToggleSwitch_OnIsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (DarkModeToggleSwitch.IsChecked is true)
        {
            App.SetDarkTheme();
        }
        else if (DarkModeToggleSwitch.IsChecked is false)
        {
            App.SetLightTheme();
        }
        else
        {
            App.SetDefaultTheme();
        }
    }
}
