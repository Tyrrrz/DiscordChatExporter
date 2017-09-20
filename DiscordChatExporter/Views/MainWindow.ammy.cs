using System.Windows.Input;
using DiscordChatExporter.ViewModels;

namespace DiscordChatExporter.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void TokenTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Execute command
                ((IMainViewModel) DataContext).PullChannelsCommand.Execute(null);
            }
        }
    }
}