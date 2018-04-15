using System;
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

            Snackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

            // Notification messages
            Messenger.Default.Register<ShowNotificationMessage>(this, m =>
            {
                if (m.CallbackCaption != null && m.Callback != null)
                    Snackbar.MessageQueue.Enqueue(m.Message, m.CallbackCaption, m.Callback);
                else
                    Snackbar.MessageQueue.Enqueue(m.Message);
            });

            // Dialog messages
            Messenger.Default.Register<ShowExportSetupMessage>(this,
                m => DialogHost.Show(new ExportSetupDialog()).Forget());
            Messenger.Default.Register<ShowSettingsMessage>(this,
                m => DialogHost.Show(new SettingsDialog()).Forget());
        }
    }
}