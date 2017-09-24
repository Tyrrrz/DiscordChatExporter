using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly Dictionary<Guild, IReadOnlyList<Channel>> _guildChannelsMap;

        private bool _isBusy;

        private IReadOnlyList<Guild> _availableGuilds;
        private Guild _selectedGuild;
        private IReadOnlyList<Channel> _availableChannels;
        private Channel _selectedChannel;

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
            get => _settingsService.Token;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                _settingsService.Token = value;
                PullChannelsCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Guild> AvailableGuilds
        {
            get => _availableGuilds;
            private set
            {
                Set(ref _availableGuilds, value);
                RaisePropertyChanged(() => AnyAvailableGuilds);
            }
        }

        public bool AnyAvailableGuilds => AvailableGuilds.NotNullAndAny();

        public Guild SelectedGuild
        {
            get => _selectedGuild;
            set
            {
                Set(ref _selectedGuild, value);
                AvailableChannels = value != null ? _guildChannelsMap[value] : new Channel[0];
                ExportChatLogCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Channel> AvailableChannels
        {
            get => _availableChannels;
            private set
            {
                Set(ref _availableChannels, value);
                RaisePropertyChanged(() => AnyAvailableChannels);
            }
        }

        public bool AnyAvailableChannels => AvailableChannels.NotNullAndAny();

        public Channel SelectedChannel
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

            _guildChannelsMap = new Dictionary<Guild, IReadOnlyList<Channel>>();

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

            // Clear existing
            _guildChannelsMap.Clear();
            AvailableGuilds = new Guild[0];
            AvailableChannels = new Channel[0];
            SelectedGuild = null;
            SelectedChannel = null;

            // Get DM channels
            var dmChannels = await _apiService.GetDirectMessageChannelsAsync(token);
            var dmGuild = new Guild("@me", "Direct Messages", "");
            _guildChannelsMap[dmGuild] = dmChannels.ToArray();

            // Get guild channels
            var guilds = await _apiService.GetGuildsAsync(token);
            foreach (var guild in guilds)
            {
                var guildChannels = await _apiService.GetGuildChannelsAsync(token, guild.Id);
                _guildChannelsMap[guild] = guildChannels.ToArray();
            }

            AvailableGuilds = _guildChannelsMap.Keys.ToArray();
            IsBusy = false;
        }

        private async void ExportChatLog()
        {
            IsBusy = true;
            var token = Token;
            var guild = SelectedGuild;
            var channel = SelectedChannel;
            
            // Get safe file names
            var safeGroupName = guild.Name;
            var safeChannelName = channel.Name;
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
            var messages = await _apiService.GetChannelMessagesAsync(token, channel.Id);

            // Create log
            var chatLog = new ChannelChatLog(guild, channel, messages);

            // Export
            _exportService.Export(sfd.FileName, chatLog, _settingsService.Theme);

            IsBusy = false;
        }
    }
}