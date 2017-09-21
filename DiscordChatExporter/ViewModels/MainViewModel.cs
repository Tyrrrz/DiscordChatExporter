using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private IReadOnlyList<ChannelViewModel> _availableChannels;
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

        public bool IsDataAvailable => AvailableChannels.NotNullAndAny();

        public string Token
        {
            get => _settingsService.Token;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                _settingsService.Token = value;
                PullChannelsCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<ChannelViewModel> AvailableChannels
        {
            get => _availableChannels;
            private set
            {
                Set(ref _availableChannels, value);
                RaisePropertyChanged(() => IsDataAvailable);
            }
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

        public RelayCommand ShowHelpCommand { get; }
        public RelayCommand PullChannelsCommand { get; }
        public RelayCommand ExportChatLogCommand { get; }

        public MainViewModel(ISettingsService settingsService, IApiService apiService, IExportService exportService)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            _exportService = exportService;

            // Commands
            ShowHelpCommand = new RelayCommand(ShowHelp);
            PullChannelsCommand = new RelayCommand(PullChannels,
                () => Token.IsNotBlank() && !IsBusy);
            ExportChatLogCommand = new RelayCommand(ExportChatLog,
                () => SelectedChannel != null && !IsBusy);
        }

        private void ShowHelp()
        {
            Process.Start("https://github.com/Tyrrrz/DiscordChatExporter/wiki");
        }

        private async void PullChannels()
        {
            IsBusy = true;
            var token = Token;
            var channelVms = new List<ChannelViewModel>();

            // Clear existing
            AvailableChannels = new ChannelViewModel[0];
            SelectedChannel = null;

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
            SelectedChannel = null;
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
            _exportService.Export(sfd.FileName, chatLog, _settingsService.Theme);

            IsBusy = false;
        }
    }
}