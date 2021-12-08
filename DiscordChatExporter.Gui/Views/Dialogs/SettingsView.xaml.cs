using System.Windows;

namespace DiscordChatExporter.Gui.Views.Dialogs;

public partial class SettingsView
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs e) =>
        App.SetDarkTheme();

    private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs e) =>
        App.SetLightTheme();
}