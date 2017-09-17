using System.Collections.Generic;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        string Token { get; set; }
        IReadOnlyList<ChannelViewModel> Channels { get; }
        ChannelViewModel SelectedChannel { get; set; }

        RelayCommand PullChannelsCommand { get; }
        RelayCommand ExportChatLogCommand { get; }
    }
}