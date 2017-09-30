using System.Collections.Generic;
using System.Windows;
using DiscordChatExporter.Models;
using DiscordChatExporter.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Views
{
    public partial class ExportSetupDialog
    {
        public ExportSetupDialog()
        {
            InitializeComponent();
        }

        private IExportSetupViewModel ViewModel => (IExportSetupViewModel) DataContext;

        private string GetOutputFileFilter()
        {
            var filters = new List<string>();
            foreach (var format in ViewModel.AvailableFormats)
            {
                var ext = format.GetFileExtension();
                filters.Add($"{format} (*.{ext})|*.{ext}");
            }

            return filters.JoinToString("|");
        }

        public void LocateFilePathButton_Click(object sender, RoutedEventArgs args)
        {
            var sfd = new SaveFileDialog
            {
                FileName = ViewModel.FilePath,
                Filter = GetOutputFileFilter(),
                FilterIndex = ViewModel.AvailableFormats.IndexOf(ViewModel.SelectedFormat) + 1,
                AddExtension = true,
                Title = "Select output file"
            };

            if (sfd.ShowDialog() == true)
            {
                ViewModel.FilePath = sfd.FileName;
                ViewModel.SelectedFormat = ViewModel.AvailableFormats[sfd.FilterIndex - 1];
            }
        }

        public void ExportButton_Click(object sender, RoutedEventArgs args)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}