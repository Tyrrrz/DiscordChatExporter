using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Framework;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class RootViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;
        private readonly DataService _dataService;
        private readonly ExportService _exportService;

        public SnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public bool IsEnabled { get; private set; } = true;

        public bool IsProgressIndeterminate => Progress < 0;

        public double Progress { get; private set; }

        public bool IsBotToken { get; set; }

        public string TokenValue { get; set; }

        public IReadOnlyList<GuildViewModel> AvailableGuilds { get; private set; }

        public GuildViewModel SelectedGuild { get; set; }

        public RootViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager,
            SettingsService settingsService, UpdateService updateService, DataService dataService,
            ExportService exportService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;
            _dataService = dataService;
            _exportService = exportService;

            // Set title
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            DisplayName = $"DiscordChatExporter v{version}";            
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();

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
                    Notifications.Enqueue(
                        $"Update to DiscordChatExporter v{updateVersion} will be installed when you exit",
                        "INSTALL NOW", () =>
                        {
                            _updateService.FinalizeUpdate(true);
                            RequestClose();
                        });
                }
            }
            catch
            {
                Notifications.Enqueue("Failed to perform application auto-update");
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            // Save settings
            _settingsService.Save();

            // Finalize updates if necessary
            _updateService.FinalizeUpdate(false);
        }

        public async void ShowSettings()
        {
            // Create dialog
            var dialog = _viewModelFactory.CreateSettingsViewModel();

            // Show dialog
            await _dialogManager.ShowDialogAsync(dialog);
        }

        public bool CanPopulateGuildsAndChannels => IsEnabled && TokenValue.IsNotBlank();

        public async void PopulateGuildsAndChannels()
        {
            try
            {
                // Set busy state and indeterminate progress
                IsEnabled = false;
                Progress = -1;

                // Sanitize token
                TokenValue = TokenValue.Trim('"');

                // Create token
                var token = new AuthToken(
                    IsBotToken ? AuthTokenType.Bot : AuthTokenType.User,
                    TokenValue);

                // Save token
                _settingsService.LastToken = token;

                // Prepare available guild list
                var availableGuilds = new List<GuildViewModel>();

                // Direct Messages
                {
                    // Get fake guild
                    var guild = Guild.DirectMessages;

                    // Get channels
                    var channels = await _dataService.GetDirectMessageChannelsAsync(token);

                    // Create channel view models
                    var channelViewModels = new List<ChannelViewModel>();
                    foreach (var channel in channels)
                    {
                        // Get fake category
                        var category = channel.Type == ChannelType.DirectTextChat ? "Private" : "Group";

                        // Create channel view model
                        var channelViewModel = _viewModelFactory.CreateChannelViewModel();
                        channelViewModel.Model = channel;
                        channelViewModel.Category = category;

                        // Add to list
                        channelViewModels.Add(channelViewModel);
                    }

                    // Create guild view model
                    var guildViewModel = _viewModelFactory.CreateGuildViewModel();
                    guildViewModel.Model = guild;
                    guildViewModel.Channels = channelViewModels.OrderBy(c => c.Category)
                        .ThenBy(c => c.Model.Name)
                        .ToArray();

                    // Add to list
                    availableGuilds.Add(guildViewModel);
                }

                // Guilds
                {
                    // Get guilds
                    var guilds = await _dataService.GetUserGuildsAsync(token);
                    foreach (var guild in guilds)
                    {
                        // Get channels
                        var channels = await _dataService.GetGuildChannelsAsync(token, guild.Id);

                        // Get category channels
                        var categoryChannels = channels.Where(c => c.Type == ChannelType.Category).ToArray();

                        // Get text channels
                        var textChannels = channels.Where(c => c.Type == ChannelType.GuildTextChat).ToArray();

                        // Create channel view models
                        var channelViewModels = new List<ChannelViewModel>();
                        foreach (var channel in textChannels)
                        {
                            // Get category
                            var category = categoryChannels.FirstOrDefault(c => c.Id == channel.ParentId)?.Name;

                            // Create channel view model
                            var channelViewModel = _viewModelFactory.CreateChannelViewModel();
                            channelViewModel.Model = channel;
                            channelViewModel.Category = category;

                            // Add to list
                            channelViewModels.Add(channelViewModel);
                        }

                        // Create guild view model
                        var guildViewModel = _viewModelFactory.CreateGuildViewModel();
                        guildViewModel.Model = guild;
                        guildViewModel.Channels = channelViewModels.OrderBy(c => c.Category)
                            .ThenBy(c => c.Model.Name)
                            .ToArray();

                        // Add to list
                        availableGuilds.Add(guildViewModel);
                    }
                }

                // Update available guild list
                AvailableGuilds = availableGuilds;

                // Pre-select first guild
                SelectedGuild = AvailableGuilds.FirstOrDefault();
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                Notifications.Enqueue("Unauthorized – make sure the token is valid");
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                Notifications.Enqueue("Forbidden – account may be locked by 2FA");
            }
            finally
            {
                // Reset busy state and progress
                Progress = 0;
                IsEnabled = true;
            }            
        }

        public bool CanExportChannel => IsEnabled;

        public async void ExportChannel(ChannelViewModel channel)
        {
            try
            {
                // Set busy state and indeterminate progress
                IsEnabled = false;
                Progress = -1;

                // Get last used token
                var token = _settingsService.LastToken;

                // Create dialog
                var dialog = _viewModelFactory.CreateExportSetupViewModel();
                dialog.Guild = SelectedGuild;
                dialog.Channel = channel;

                // Show dialog, if canceled - return
                if (await _dialogManager.ShowDialogAsync(dialog) != true)
                    return;

                // Create progress handler
                var progressHandler = new Progress<double>(p => Progress = p);

                // Get chat log
                var chatLog = await _dataService.GetChatLogAsync(token, dialog.Guild.Model, dialog.Channel.Model,
                    dialog.From, dialog.To, progressHandler);

                // Export
                _exportService.ExportChatLog(chatLog, dialog.FilePath, dialog.SelectedFormat,
                    dialog.PartitionLimit);

                // Notify completion
                Notifications.Enqueue("Export complete");
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                Notifications.Enqueue("You don't have access to this channel");
            }
            catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Notifications.Enqueue("This channel doesn't exist");
            }
            finally
            {
                // Reset busy state and progress
                Progress = 0;
                IsEnabled = true;
            }
        }
    }
}