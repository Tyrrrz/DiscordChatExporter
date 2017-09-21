using System.Collections.Generic;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        bool IsDataAvailable { get; }

        string Token { get; set; }
        IReadOnlyList<ChannelViewModel> AvailableChannels { get; }
        ChannelViewModel SelectedChannel { get; set; }

        RelayCommand ShowHelpCommand { get; }
        RelayCommand PullChannelsCommand { get; }
        RelayCommand ExportChatLogCommand { get; }
    }
}