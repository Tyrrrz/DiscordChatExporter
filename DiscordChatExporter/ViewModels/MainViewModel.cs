using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using DiscordChatExporter.Exceptions;
using DiscordChatExporter.Messages;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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
                ShowExportSetupCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsDataAvailable => AvailableGuilds.NotNullAndAny();

        public string Token
        {
            get => _settingsService.LastToken;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                _settingsService.LastToken = value;
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
                ShowExportSetupCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Channel> AvailableChannels
        {
            get => _availableChannels;
            private set => Set(ref _availableChannels, value);
        }

        public RelayCommand PullDataCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }
        public RelayCommand ShowAboutCommand { get; }
        public RelayCommand<Channel> ShowExportSetupCommand { get; }

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
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowExportSetupCommand = new RelayCommand<Channel>(ShowExportSetup, _ => !IsBusy);

            // Messages
            MessengerInstance.Register<StartExportMessage>(this, m =>
            {
                Export(m.Channel, m.FilePath, m.Format, m.From, m.To);
            });
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

        private void ShowSettings()
        {
            MessengerInstance.Send(new ShowSettingsMessage());
        }

        private void ShowAbout()
        {
            Process.Start("https://github.com/Tyrrrz/DiscordChatExporter");
        }

        private void ShowExportSetup(Channel channel)
        {
            MessengerInstance.Send(new ShowExportSetupMessage(SelectedGuild, channel));
        }

        private async void Export(Channel channel, string filePath, ExportFormat format, DateTime? from, DateTime? to)
        {
            IsBusy = true;

            try
            {
                // Get messages
                var messages = await _dataService.GetChannelMessagesAsync(_cachedToken, channel.Id, from, to);

                // Group them
                var messageGroups = _messageGroupService.GroupMessages(messages);

                // Create log
                var chatLog = new ChannelChatLog(SelectedGuild, channel, messageGroups, messages.Count);

                // Export
                if (format == ExportFormat.Text)
                    await _exportService.ExportAsTextAsync(filePath, chatLog);
                else if (format == ExportFormat.Html)
                    await _exportService.ExportAsHtmlAsync(filePath, chatLog, _settingsService.Theme);

                // Notify completion
                MessengerInstance.Send(new ShowExportDoneMessage(filePath));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                const string message = "You don't have access to the messages in that channel.";
                MessengerInstance.Send(new ShowErrorMessage(message));
            }

            IsBusy = false;
        }
    }
}