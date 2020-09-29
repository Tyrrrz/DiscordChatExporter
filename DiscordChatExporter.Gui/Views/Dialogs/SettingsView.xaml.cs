using MaterialDesignThemes;
using MaterialDesignThemes.Wpf;
using System.Windows.Media.Media3D;

namespace DiscordChatExporter.Gui.Views.Dialogs
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            setBaseTheme(Theme.Dark);
        }

        private void ToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            setBaseTheme(Theme.Light);
        }

        private void setBaseTheme(IBaseTheme baseTheme)
        {
            ((App)System.Windows.Application.Current).setBaseTheme(baseTheme);
        }
    }
}