using System.Windows;
using System.Windows.Input;
using DiscordChatExporter.ViewModels;
using MaterialDesignThemes.Wpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Views
{
    public partial class MainWindow
    {
        private IMainViewModel ViewModel => (IMainViewModel) DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void TokenTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Execute command
                ViewModel.PullDataCommand.Execute(null);
            }
        }

        public void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Show(new SettingsDialog()).Forget();
        }
    }
}