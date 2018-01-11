using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IExportDoneViewModel
    {
        RelayCommand OpenCommand { get; }
    }
}