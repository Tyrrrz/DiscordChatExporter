using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.Gui.ViewModels
{
    public interface IExportDoneViewModel
    {
        RelayCommand OpenCommand { get; }
    }
}