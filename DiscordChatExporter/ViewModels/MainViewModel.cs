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

            // Clear existing
            _guildChannelsMap.Clear();
            AvailableGuilds = new Guild[0];
            AvailableChannels = new Channel[0];
            SelectedGuild = null;

            // Get DM channels
            var dmChannels = await _dataService.GetDirectMessageChannelsAsync(Token);
            var dmGuild = new Guild("@me", "Direct Messages", null);
            _guildChannelsMap[dmGuild] = dmChannels.ToArray();

            // Get guild channels
            var guilds = await _dataService.GetGuildsAsync(Token);
            foreach (var guild in guilds)
            {
                var guildChannels = await _dataService.GetGuildChannelsAsync(Token, guild.Id);
                guildChannels = guildChannels.Where(c => c.Type == ChannelType.GuildTextChat);
                _guildChannelsMap[guild] = guildChannels.ToArray();
            }

            AvailableGuilds = _guildChannelsMap.Keys.ToArray();
            SelectedGuild = AvailableGuilds.FirstOrDefault();
            IsBusy = false;
        }

        private async void ExportChannel(Channel channel)
        {
            IsBusy = true;
            
            // Get safe file names
            var safeGroupName = SelectedGuild.Name;
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
            var messages = await _dataService.GetChannelMessagesAsync(Token, channel.Id);

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