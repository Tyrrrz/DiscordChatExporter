using System.Reflection;
using DiscordChatExporter.Gui.Messages;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Title += $" v{Assembly.GetExecutingAssembly().GetName().Version}";

            // Dialogs
            Messenger.Default.Register<ShowErrorMessage>(this,
                m => DialogHost.Show(new ErrorDialog()).Forget());
            Messenger.Default.Register<ShowExportDoneMessage>(this,
                m => DialogHost.Show(new ExportDoneDialog()).Forget());
            Messenger.Default.Register<ShowExportSetupMessage>(this,
                m => DialogHost.Show(new ExportSetupDialog()).Forget());
            Messenger.Default.Register<ShowSettingsMessage>(this,
                m => DialogHost.Show(new SettingsDialog()).Forget());
        }
    }
}