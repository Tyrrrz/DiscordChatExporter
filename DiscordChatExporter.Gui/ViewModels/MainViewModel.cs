using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;
        private readonly IDataService _dataService;
        private readonly IMessageGroupService _messageGroupService;
        private readonly IExportService _exportService;

        private readonly Dictionary<Guild, IReadOnlyList<Channel>> _guildChannelsMap;

        private bool _isBusy;
        private double _progress;
        private bool _isBotToken;
        private string _tokenValue;
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
                ShowExportSetupCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsDataAvailable => AvailableGuilds.NotNullAndAny();

        public bool IsProgressIndeterminate => Progress <= 0;

        public double Progress
        {
            get => _progress;
            private set
            {
                Set(ref _progress, value);
                RaisePropertyChanged(() => IsProgressIndeterminate);
            }
        }

        public bool IsBotToken
        {
            get => _isBotToken;
            set => Set(ref _isBotToken, value);
        }

        public string TokenValue
        {
            get => _tokenValue;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                Set(ref _tokenValue, value);
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
                AvailableChannels = value != null ? _guildChannelsMap[value] : Array.Empty<Channel>();
                ShowExportSetupCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<Channel> AvailableChannels
        {
            get => _availableChannels;
            private set => Set(ref _availableChannels, value);
        }

        public RelayCommand ViewLoadedCommand { get; }
        public RelayCommand ViewClosedCommand { get; }
        public RelayCommand PullDataCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }
        public RelayCommand ShowAboutCommand { get; }
        public RelayCommand<Channel> ShowExportSetupCommand { get; }

        public MainViewModel(ISettingsService settingsService, IUpdateService updateService, IDataService dataService,
            IMessageGroupService messageGroupService, IExportService exportService)
        {
            _settingsService = settingsService;
            _updateService = updateService;
            _dataService = dataService;
            _messageGroupService = messageGroupService;
            _exportService = exportService;

            _guildChannelsMap = new Dictionary<Guild, IReadOnlyList<Channel>>();

            // Commands
            ViewLoadedCommand = new RelayCommand(ViewLoaded);
            ViewClosedCommand = new RelayCommand(ViewClosed);
            PullDataCommand = new RelayCommand(PullData, () => TokenValue.IsNotBlank() && !IsBusy);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowExportSetupCommand = new RelayCommand<Channel>(ShowExportSetup, _ => !IsBusy);

            // Messages
            MessengerInstance.Register<StartExportMessage>(this,
                m => Export(m.Channel, m.FilePath, m.Format, m.From, m.To));
        }

        private async void ViewLoaded()
        {
            // Load settings
            _settingsService.Load();

            // Get last token
            if (_settingsService.LastToken != null)
            {
                IsBotToken = _settingsService.LastToken.Type == AuthTokenType.Bot;
                TokenValue = _settingsService.LastToken.Value;
            }

            // Check and prepare update
            try
            {
                var updateVersion = await _updateService.CheckPrepareUpdateAsync();
                if (updateVersion != null)
                {
                    MessengerInstance.Send(new ShowNotificationMessage(
                        $"Update to DiscordChatExporter v{updateVersion} will be installed when you exit",
                        "INSTALL NOW", () =>
                        {
                            _updateService.NeedRestart = true;
                            Application.Current.Shutdown();
                        }));
                }
            }
            catch
            {
                MessengerInstance.Send(new ShowNotificationMessage("Failed to perform application auto-update"));
            }
        }

        private void ViewClosed()
        {
            // Save settings
            _settingsService.Save();

            // Finalize updates if available
            _updateService.FinalizeUpdate();
        }

        private async void PullData()
        {
            IsBusy = true;

            // Create token
            var tokenType = IsBotToken ? AuthTokenType.Bot : AuthTokenType.User;
            var token = new AuthToken(tokenType, TokenValue);

            // Save token
            _settingsService.LastToken = token;

            // Clear existing
            _guildChannelsMap.Clear();

            try
            {
                // Get DM channels
                {
                    var channels = await _dataService.GetDirectMessageChannelsAsync(token);
                    var guild = Guild.DirectMessages;
                    _guildChannelsMap[guild] = channels.OrderBy(c => c.Name).ToArray();
                }

                // Get guild channels
                {
                    var guilds = await _dataService.GetUserGuildsAsync(token);
                    foreach (var guild in guilds)
                    {
                        var channels = await _dataService.GetGuildChannelsAsync(token, guild.Id);
                        _guildChannelsMap[guild] = channels.Where(c => c.Type == ChannelType.GuildTextChat)
                            .OrderBy(c => c.Name)
                            .ToArray();
                    }
                }
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                MessengerInstance.Send(new ShowNotificationMessage("Unauthorized – make sure the token is valid"));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                MessengerInstance.Send(new ShowNotificationMessage("Forbidden – account may be locked by 2FA"));
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

            // Get last used token
            var token = _settingsService.LastToken;

            // Get guild
            var guild = SelectedGuild;

            // Create progress handler
            var progressHandler = new Progress<double>(p => Progress = p);

            try
            {
                // Get messages
                var messages = await _dataService.GetChannelMessagesAsync(token, channel.Id, from, to, progressHandler);

                // Group messages
                var messageGroups = _messageGroupService.GroupMessages(messages);

                // Get mentionables
                var mentionables = await _dataService.GetMentionablesAsync(token, guild.Id, messages);

                // Create log
                var log = new ChatLog(guild, channel, from, to, messageGroups, mentionables);

                // Export
                _exportService.Export(format, filePath, log);

                // Open
                Process.Start(filePath);

                // Notify completion
                MessengerInstance.Send(new ShowNotificationMessage("Export complete"));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                MessengerInstance.Send(new ShowNotificationMessage("You don't have access to this channel"));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                MessengerInstance.Send(new ShowNotificationMessage("This channel doesn't exist"));
            }

            Progress = 0;
            IsBusy = false;
        }
    }
}