using System.Windows;
using MaterialDesignThemes.Wpf;

namespace DiscordChatExporter.Gui.Views
{
    public partial class ExportDoneDialog
    {
        public ExportDoneDialog()
        {
            InitializeComponent();
        }

        public void OpenButton_Click(object sender, RoutedEventArgs args)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
