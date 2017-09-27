using System.Reflection;
using System.Windows.Input;
using DiscordChatExporter.Messages;
using DiscordChatExporter.ViewModels;
using GalaSoft.MvvmLight.Messaging;
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
            Title += $" v{Assembly.GetExecutingAssembly().GetName().Version}";

            Messenger.Default.Register<ShowSettingsMessage>(this, m => DialogHost.Show(new SettingsDialog()).Forget());
        }

        public void TokenTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Execute command
                ViewModel.PullDataCommand.Execute(null);
            }
        }
    }
}