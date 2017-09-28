using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DiscordChatExporter.Messages;
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
        private readonly IDataService _dataService;
        private readonly IExportService _exportService;

        private readonly Dictionary<Guild, IReadOnlyList<Channel>> _guildChannelsMap;

        private bool _isBusy;
        private IReadOnlyList<Guild> _availableGuilds;
        private Guild _selectedGuild;
        private IReadOnlyList<Channel> _availableChannels;
        private string _cachedToken;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                Set(ref _isBusy, value);
                PullDataCommand.RaiseCanExecuteChanged();
                ExportChannelCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsDataAvailable => AvailableGuilds.NotNullAndAny();

        public string Token
        {
            get => _settingsService.Token;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                _settingsService.Token = value;
                PullDataCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Guild> AvailableGuilds
        {
            get => _availableGuilds;
            private set
            {
                Set(ref _availableGuilds, value);
                RaisePropertyChanged(() => IsDataAvailable);
            }
        }

        public Guild SelectedGuild
        {
            get => _selectedGuild;
            set
            {
                Set(ref _selectedGuild, value);
                AvailableChannels = value != null ? _guildChannelsMap[value] : new Channel[0];
                ExportChannelCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Channel> AvailableChannels
        {
            get => _availableChannels;
            private set => Set(ref _availableChannels, value);
        }

        public RelayCommand PullDataCommand { get; }
        public RelayCommand<Channel> ExportChannelCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }
        public RelayCommand ShowAboutCommand { get; }

        public MainViewModel(ISettingsService settingsService, IDataService dataService, IExportService exportService)
        {
            _settingsService = settingsService;
            _dataService = dataService;
            _exportService = exportService;

            _guildChannelsMap = new Dictionary<Guild, IReadOnlyList<Channel>>();

            // Commands
            PullDataCommand = new RelayCommand(PullData, () => Token.IsNotBlank() && !IsBusy);
            ExportChannelCommand = new RelayCommand<Channel>(ExportChannel, _ => !IsBusy);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowAboutCommand = new RelayCommand(ShowAbout);
        }

        private async void PullData()
        {
            IsBusy = true;
            _cachedToken = Token;

            // Clear existing
            _guildChannelsMap.Clear();

            // Get DM channels
            {
                var channels = await _dataService.GetDirectMessageChannelsAsync(_cachedToken);
                var guild = new Guild("@me", "Direct Messages", null);
                _guildChannelsMap[guild] = channels.ToArray();
            }

            // Get guild channels
            {
                var guilds = await _dataService.GetGuildsAsync(_cachedToken);
                foreach (var guild in guilds)
                {
                    var channels = await _dataService.GetGuildChannelsAsync(_cachedToken, guild.Id);
                    channels = channels.Where(c => c.Type == ChannelType.GuildTextChat);
                    _guildChannelsMap[guild] = channels.ToArray();
                }
            }

            AvailableGuilds = _guildChannelsMap.Keys.ToArray();
            SelectedGuild = AvailableGuilds.FirstOrDefault();
            IsBusy = false;
        }

        private async void ExportChannel(Channel channel)
        {
            IsBusy = true;
            
            // Get safe file names
            var safeGroupName = SelectedGuild.Name.Replace(Path.GetInvalidFileNameChars(), '_');
            var safeChannelName = channel.Name.Replace(Path.GetInvalidFileNameChars(), '_');

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
            var messages = await _dataService.GetChannelMessagesAsync(_cachedToken, channel.Id);

            // Create log
            var chatLog = new ChannelChatLog(SelectedGuild, channel, messages);

            // Export
            _exportService.Export(sfd.FileName, chatLog, _settingsService.Theme);

            IsBusy = false;
        }

        private void ShowSettings()
        {
            MessengerInstance.Send(new ShowSettingsMessage());
        }

        private void ShowAbout()
        {
            Process.Start("https://github.com/Tyrrrz/DiscordChatExporter");
        }
    }
}