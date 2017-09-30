using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using DiscordChatExporter.Exceptions;
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
        private readonly IMessageGroupService _messageGroupService;
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

        public MainViewModel(ISettingsService settingsService, IDataService dataService,
            IMessageGroupService messageGroupService, IExportService exportService)
        {
            _settingsService = settingsService;
            _dataService = dataService;
            _messageGroupService = messageGroupService;
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

            try
            {
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
                        _guildChannelsMap[guild] = channels.Where(c => c.Type == ChannelType.GuildTextChat).ToArray();
                    }
                }
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                const string message = "Could not authorize using the given token. Make sure it's valid.";
                MessengerInstance.Send(new ShowErrorMessage(message));
            }

            AvailableGuilds = _guildChannelsMap.Keys.ToArray();
            SelectedGuild = AvailableGuilds.FirstOrDefault();
            IsBusy = false;
        }

        private async void ExportChannel(Channel channel)
        {
            IsBusy = true;
            
            // Get safe file names
            var safeGuildName = SelectedGuild.Name.Replace(Path.GetInvalidFileNameChars(), '_');
            var safeChannelName = channel.Name.Replace(Path.GetInvalidFileNameChars(), '_');

            // Ask for path
            var sfd = new SaveFileDialog
            {
                FileName = $"{safeGuildName} - {safeChannelName}.html",
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = "html",
                AddExtension = true
            };
            if (sfd.ShowDialog() != true)
            {
                IsBusy = false;
                return;
            }

            // Export
            try
            {
                // Get messages
                var messages = await _dataService.GetChannelMessagesAsync(_cachedToken, channel.Id);

                // Group them
                var messageGroups = _messageGroupService.GroupMessages(messages);

                // Create log
                var chatLog = new ChannelChatLog(SelectedGuild, channel, messageGroups, messages.Count);

                // Export
                await _exportService.ExportAsync(sfd.FileName, chatLog, _settingsService.Theme);

                // Show dialog
                MessengerInstance.Send(new ShowExportDoneMessage(sfd.FileName));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                const string message = "You don't have access to the messages in that channel.";
                MessengerInstance.Send(new ShowErrorMessage(message));
            }

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