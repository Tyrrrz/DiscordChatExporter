using System.Collections.Generic;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly IApiService _apiService;
        private readonly IExportService _exportService;

        private bool _isBusy;
        private string _token;
        private IReadOnlyList<ChannelViewModel> _channels;
        private ChannelViewModel _selectedChannel;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                Set(ref _isBusy, value);
                PullChannelsCommand.RaiseCanExecuteChanged();
                ExportChatLogCommand.RaiseCanExecuteChanged();
            }
        }

        public string Token
        {
            get => _token;
            set
            {
                Set(ref _token, value);
                PullChannelsCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<ChannelViewModel> Channels
        {
            get => _channels;
            private set => Set(ref _channels, value);
        }

        public ChannelViewModel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                Set(ref _selectedChannel, value);
                ExportChatLogCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand PullChannelsCommand { get; }
        public RelayCommand ExportChatLogCommand { get; }

        public MainViewModel(IApiService apiService, IExportService exportService)
        {
            _apiService = apiService;
            _exportService = exportService;

            // Commands
            PullChannelsCommand = new RelayCommand(PullChannels, () => Token.IsNotBlank() && !IsBusy);
            ExportChatLogCommand = new RelayCommand(ExportChatLog, () => SelectedChannel != null && !IsBusy);
        }

        private async void PullChannels()
        {
            IsBusy = true;
            var token = Token.Trim('"');
            var channelVms = new List<ChannelViewModel>();

            // Clear existing
            Channels = new ChannelViewModel[0];

            // Get DM channels
            var dmChannels = await _apiService.GetDirectMessageChannelsAsync(token);
            foreach (var channel in dmChannels)
            {
                var channelVm = new ChannelViewModel("Direct Messages", channel);
                channelVms.Add(channelVm);
            }

            // Get guild channels
            var guilds = await _apiService.GetGuildsAsync(token);
            foreach (var guild in guilds)
            {
                var guildChannels = await _apiService.GetGuildChannelsAsync(token, guild.Id);
                foreach (var channel in guildChannels)
                {
                    var channelVm = new ChannelViewModel(guild.Name, channel);
                    channelVms.Add(channelVm);
                }
            }

            Channels = channelVms;

            IsBusy = false;
        }

        private async void ExportChatLog()
        {
            IsBusy = true;
            var token = Token.Trim('"');
            var channel = SelectedChannel.Channel;

            // Get messages
            var messages = await _apiService.GetChannelMessagesAsync(token, channel.Id);

            // Create log
            var chatLog = new ChatLog(channel.Id, messages);

            // Export
            _exportService.Export($"{channel.Name}.html", chatLog, Theme.Dark);

            IsBusy = false;
        }
    }
}