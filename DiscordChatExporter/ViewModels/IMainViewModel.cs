using System.Collections.Generic;
using DiscordChatExporter.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }

        string Token { get; set; }

        IReadOnlyList<Guild> AvailableGuilds { get; }
        bool AnyAvailableGuilds { get; }
        Guild SelectedGuild { get; set; }

        IReadOnlyList<Channel> AvailableChannels { get; }
        bool AnyAvailableChannels { get; }
        Channel SelectedChannel { get; set; }

        RelayCommand ShowHelpCommand { get; }
        RelayCommand PullChannelsCommand { get; }
        RelayCommand ExportChatLogCommand { get; }
    }
}