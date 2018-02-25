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
        private string _token;
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

        public string Token
        {
            get => _token;
            set
            {
                // Remove invalid chars
                value = value?.Trim('"');

                Set(ref _token, value);
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
            PullDataCommand = new RelayCommand(PullData, () => Token.IsNotBlank() && !IsBusy);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowExportSetupCommand = new RelayCommand<Channel>(ShowExportSetup, _ => !IsBusy);

            // Messages
            MessengerInstance.Register<StartExportMessage>(this,
                m => { Export(m.Channel, m.FilePath, m.Format, m.From, m.To); });
        }

        private async void ViewLoaded()
        {
            // Load settings
            _settingsService.Load();

            // Set last token
            Token = _settingsService.LastToken;

            // Check for updates
            var lastVersion = await _updateService.CheckForUpdatesAsync();
            if (lastVersion != null)
            {
                // Download updates
                await _updateService.PrepareUpdateAsync();

                // Notify user
                MessengerInstance.Send(
                    new ShowNotificationMessage(
                        $"DiscordChatExporter v{lastVersion} has been downloaded. " +
                        "It will be installed once you exit.",
                        "INSTALL NOW",
                        async () =>
                        {
                            await _updateService.ApplyUpdateAsync();
                            Application.Current.Shutdown();
                        }));
            }
        }

        private void ViewClosed()
        {
            // Save settings
            _settingsService.Save();

            // Apply updates if available
            _updateService.ApplyUpdateAsync(false);
        }

        private async void PullData()
        {
            IsBusy = true;

            // Copy token so it doesn't get mutated
            var token = Token;

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
                    _guildChannelsMap[guild] = channels.ToArray();
                }

                // Get guild channels
                {
                    var guilds = await _dataService.GetUserGuildsAsync(token);
                    foreach (var guild in guilds)
                    {
                        var channels = await _dataService.GetGuildChannelsAsync(token, guild.Id);
                        _guildChannelsMap[guild] = channels.Where(c => c.Type == ChannelType.GuildTextChat).ToArray();
                    }
                }
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                const string message = "Unauthorized to perform request. Make sure token is valid.";
                MessengerInstance.Send(new ShowErrorMessage(message));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                const string message = "Forbidden to perform request. The account may be locked by 2FA.";
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

            // Get last used token
            var token = _settingsService.LastToken;

            try
            {
                // Get messages
                var messages = await _dataService.GetChannelMessagesAsync(token, channel.Id, from, to);

                // Group them
                var messageGroups = _messageGroupService.GroupMessages(messages);

                // Create log
                var log = new ChannelChatLog(SelectedGuild, channel, messageGroups, messages.Count);

                // Export
                await _exportService.ExportAsync(format, filePath, log);

                // Notify completion
                MessengerInstance.Send(new ShowExportDoneMessage(filePath));
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                const string message = "Forbidden to view messages in that channel.";
                MessengerInstance.Send(new ShowErrorMessage(message));
            }

            IsBusy = false;
        }
    }
}