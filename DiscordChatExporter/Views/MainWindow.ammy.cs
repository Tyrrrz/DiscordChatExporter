using System.Windows.Input;
using DiscordChatExporter.ViewModels;

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
                ViewModel.PullChannelsCommand.Execute(null);
            }
        }
    }
}