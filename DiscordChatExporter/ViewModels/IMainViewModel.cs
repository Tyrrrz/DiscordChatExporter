using System.Collections.Generic;
using DiscordChatExporter.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        bool IsDataAvailable { get; }

        string Token { get; set; }

        IReadOnlyList<Guild> AvailableGuilds { get; }
        Guild SelectedGuild { get; set; }

        IReadOnlyList<Channel> AvailableChannels { get; }
        Channel SelectedChannel { get; set; }

        RelayCommand ShowHelpCommand { get; }
        RelayCommand PullChannelsCommand { get; }
        RelayCommand ExportChatLogCommand { get; }
    }
}