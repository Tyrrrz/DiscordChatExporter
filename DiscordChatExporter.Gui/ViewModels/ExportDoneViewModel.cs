using System.Diagnostics;
using DiscordChatExporter.Gui.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class ExportDoneViewModel : ViewModelBase, IExportDoneViewModel
    {
        private string _filePath;

        // Commands
        public RelayCommand OpenCommand { get; }

        public ExportDoneViewModel()
        {
            // Commands
            OpenCommand = new RelayCommand(Open);

            // Messages
            MessengerInstance.Register<ShowExportDoneMessage>(this, m =>
            {
                _filePath = m.FilePath;
            });
        }

        private void Open()
        {
            Process.Start(_filePath);
        }
    }
}