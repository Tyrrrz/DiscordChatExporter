using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IApiService _apiService;
        private readonly IExportService _exportService;

        private bool _isBusy;
        private string _token;
        private IReadOnlyList<ChannelViewModel> _availableChannels;
        private ChannelViewModel _selectedChannel;
        private Theme _selectedTheme;

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
                // Remove invalid chars
                value = value?.Trim('"');

                Set(ref _token, value);
                _settingsService.Token = value;
                PullChannelsCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<ChannelViewModel> AvailableChannels
        {
            get => _availableChannels;
            private set => Set(ref _availableChannels, value);
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

        public IReadOnlyList<Theme> AvailableThemes { get; }

        public Theme SelectedTheme
        {
            get => _selectedTheme;
            set => Set(ref _selectedTheme, value);
        }

        public RelayCommand PullChannelsCommand { get; }
        public RelayCommand ExportChatLogCommand { get; }

        public MainViewModel(ISettingsService settingsService, IApiService apiService, IExportService exportService)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            _exportService = exportService;

            // Defaults
            _token = _settingsService.Token;
            AvailableThemes = Enum.GetValues(typeof(Theme)).Cast<Theme>().ToArray();

            // Commands
            PullChannelsCommand = new RelayCommand(PullChannels,
                () => Token.IsNotBlank() && !IsBusy);
            ExportChatLogCommand = new RelayCommand(ExportChatLog,
                () => SelectedChannel != null && !IsBusy);
        }

        private async void PullChannels()
        {
            IsBusy = true;
            var token = Token;
            var channelVms = new List<ChannelViewModel>();

            // Clear existing
            AvailableChannels = new ChannelViewModel[0];

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

            AvailableChannels = channelVms;
            IsBusy = false;
        }

        private async void ExportChatLog()
        {
            IsBusy = true;
            var token = Token;
            var channelVm = SelectedChannel;
            
            // Get safe file names
            var safeGroupName = channelVm.GroupName;
            var safeChannelName = channelVm.Channel.Name;
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                safeGroupName = safeGroupName.Replace(invalidChar, '_');
                safeChannelName = safeChannelName.Replace(invalidChar, '_');
            }

            // Ask for path
            var sfd = new SaveFileDialog
            {
                FileName = $"{safeGroupName} - {safeChannelName}.html",
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = "html",
                AddExtension = true
            };
            if (sfd.ShowDialog() != true)
            {
                IsBusy = false;
                return;
            }

            // Get messages
            var messages = await _apiService.GetChannelMessagesAsync(token, channelVm.Channel.Id);

            // Create log
            var chatLog = new ChatLog(channelVm.Channel.Id, messages);

            // Export
            _exportService.Export(sfd.FileName, chatLog, SelectedTheme);

            IsBusy = false;
        }
    }
}