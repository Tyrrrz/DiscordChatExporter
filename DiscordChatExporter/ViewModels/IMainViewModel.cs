using System.Collections.Generic;
using DiscordChatExporter.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }

        string Token { get; set; }
        IReadOnlyList<ChannelViewModel> AvailableChannels { get; }
        ChannelViewModel SelectedChannel { get; set; }

        IReadOnlyList<Theme> AvailableThemes { get; }
        Theme SelectedTheme { get; set; }

        RelayCommand PullChannelsCommand { get; }
        RelayCommand ExportChatLogCommand { get; }
    }
}