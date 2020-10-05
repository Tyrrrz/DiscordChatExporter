using System.Windows;

namespace DiscordChatExporter.Gui.Views.Dialogs
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            App.SetDarkTheme();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            App.SetLightTheme();
        }
    }
}