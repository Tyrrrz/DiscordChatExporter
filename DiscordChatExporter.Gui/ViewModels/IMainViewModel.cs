using System.Collections.Generic;
using DiscordChatExporter.Core.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace DiscordChatExporter.Gui.ViewModels
{
    public interface IMainViewModel
    {
        bool IsBusy { get; }
        bool IsDataAvailable { get; }

        bool IsProgressIndeterminate { get; }
        double Progress { get; }

        IReadOnlyList<AuthTokenType> AvailableTokenTypes { get; }
        AuthTokenType TokenType { get; set; }
        string TokenValue { get; set; }

        IReadOnlyList<Guild> AvailableGuilds { get; }
        Guild SelectedGuild { get; set; }
        IReadOnlyList<Channel> AvailableChannels { get; }

        RelayCommand ViewLoadedCommand { get; }
        RelayCommand ViewClosedCommand { get; }
        RelayCommand PullDataCommand { get; }
        RelayCommand ShowSettingsCommand { get; }
        RelayCommand ShowAboutCommand { get; }
        RelayCommand<Channel> ShowExportSetupCommand { get; }
    }
}