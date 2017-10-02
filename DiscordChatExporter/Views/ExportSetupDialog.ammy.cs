using System.Windows;
using DiscordChatExporter.Models;
using DiscordChatExporter.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace DiscordChatExporter.Views
{
    public partial class ExportSetupDialog
    {
        private IExportSetupViewModel ViewModel => (IExportSetupViewModel) DataContext;

        public ExportSetupDialog()
        {
            InitializeComponent();
        }

        public void BrowseButton_Click(object sender, RoutedEventArgs args)
        {
            // Get file extension of the selected format
            var ext = ViewModel.SelectedFormat.GetFileExtension();

            // Open dialog
            var sfd = new SaveFileDialog
            {
                FileName = ViewModel.FilePath,
                Filter = $"{ext.ToUpperInvariant()} Files|*.{ext}|All Files|*.*",
                AddExtension = true,
                Title = "Select output file"
            };

            // Assign new file path if dialog was successful
            if (sfd.ShowDialog() == true)
            {
                ViewModel.FilePath = sfd.FileName;
            }
        }

        public void ExportButton_Click(object sender, RoutedEventArgs args)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}