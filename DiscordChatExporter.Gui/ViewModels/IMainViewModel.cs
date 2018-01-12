using System.Collections.Generic;
using DiscordChatExporter.Core.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.Gui.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        bool IsDataAvailable { get; }

        string Token { get; set; }

        IReadOnlyList<Guild> AvailableGuilds { get; }
        Guild SelectedGuild { get; set; }
        IReadOnlyList<Channel> AvailableChannels { get; }

        RelayCommand PullDataCommand { get; }
        RelayCommand ShowSettingsCommand { get; }
        RelayCommand ShowAboutCommand { get; }
        RelayCommand<Channel> ShowExportSetupCommand { get; }
    }
}