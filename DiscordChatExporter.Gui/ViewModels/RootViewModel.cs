using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Exporting;
using DiscordChatExporter.Domain.Utilities;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Gress;
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

        public ISnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public IProgressManager ProgressManager { get; } = new ProgressManager();

        public bool IsBusy { get; private set; }

        public bool IsProgressIndeterminate { get; private set; }

        public bool IsBotToken { get; set; }

        public string? TokenValue { get; set; }

        public IReadOnlyList<GuildViewModel>? AvailableGuilds { get; private set; }

        public GuildViewModel? SelectedGuild { get; set; }

        public IReadOnlyList<ChannelViewModel>? SelectedChannels { get; set; }

        public RootViewModel(
            IViewModelFactory viewModelFactory,
            DialogManager dialogManager,
            SettingsService settingsService,
            UpdateService updateService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;

            DisplayName = $"{App.Name} v{App.VersionString}";

            // Update busy state when progress manager changes
            ProgressManager.Bind(o => o.IsActive,
                (sender, args) => IsBusy = ProgressManager.IsActive);
            ProgressManager.Bind(o => o.IsActive,
                (sender, args) => IsProgressIndeterminate = ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1));
            ProgressManager.Bind(o => o.Progress,
                (sender, args) => IsProgressIndeterminate = ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1));
        }

        private async Task HandleAutoUpdateAsync()
        {
            try
            {
                var updateVersion = await _updateService.CheckForUpdatesAsync();
                if (updateVersion == null)
                    return;

                Notifications.Enqueue($"Downloading update to {App.Name} v{updateVersion}...");
                await _updateService.PrepareUpdateAsync(updateVersion);

                Notifications.Enqueue(
                    "Update has been downloaded and will be installed when you exit",
                    "INSTALL NOW", () =>
                    {
                        _updateService.FinalizeUpdate(true);
                        RequestClose();
                    });
            }
            catch
            {
                // Failure to update shouldn't crash the application
                Notifications.Enqueue("Failed to perform application update");
            }
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();

            _settingsService.Load();

            if (_settingsService.LastToken != null)
            {
                IsBotToken = _settingsService.LastToken.Type == AuthTokenType.Bot;
                TokenValue = _settingsService.LastToken.Value;
            }

            await HandleAutoUpdateAsync();
        }

        protected override void OnClose()
        {
            base.OnClose();

            _settingsService.Save();
            _updateService.FinalizeUpdate(false);
        }

        public async void ShowSettings()
        {
            var dialog = _viewModelFactory.CreateSettingsViewModel();
            await _dialogManager.ShowDialogAsync(dialog);
        }

        public bool CanPopulateGuildsAndChannels =>
            !IsBusy && !string.IsNullOrWhiteSpace(TokenValue);

        public async void PopulateGuildsAndChannels()
        {
            using var operation = ProgressManager.CreateOperation();

            try
            {
                var tokenValue = TokenValue?.Trim('"');
                if (string.IsNullOrWhiteSpace(tokenValue))
                    return;

                var token = new AuthToken(
                    IsBotToken ? AuthTokenType.Bot : AuthTokenType.User,
                    tokenValue
                );

                _settingsService.LastToken = token;

                var discord = new DiscordClient(token);

                var availableGuilds = new List<GuildViewModel>();

                // Direct messages
                {
                    var guild = Guild.DirectMessages;
                    var channels = await discord.GetDirectMessageChannelsAsync();

                    // Create channel view models
                    var channelViewModels = new List<ChannelViewModel>();
                    foreach (var channel in channels)
                    {
                        // Get fake category
                        var category = channel.Type == ChannelType.DirectTextChat ? "Private" : "Group";

                        // Create channel view model
                        var channelViewModel = _viewModelFactory.CreateChannelViewModel(channel, category);

                        // Add to list
                        channelViewModels.Add(channelViewModel);
                    }

                    // Create guild view model
                    var guildViewModel = _viewModelFactory.CreateGuildViewModel(guild,
                        channelViewModels.OrderBy(c => c.Category)
                            .ThenBy(c => c.Model!.Name)
                            .ToArray());

                    // Add to list
                    availableGuilds.Add(guildViewModel);
                }

                // Guilds
                var guilds = await discord.GetUserGuildsAsync();
                foreach (var guild in guilds)
                {
                    var channels = await discord.GetGuildChannelsAsync(guild.Id);
                    var categoryChannels = channels.Where(c => c.Type == ChannelType.GuildCategory).ToArray();
                    var exportableChannels = channels.Where(c => c.IsTextChannel).ToArray();

                    // Create channel view models
                    var channelViewModels = new List<ChannelViewModel>();
                    foreach (var channel in exportableChannels)
                    {
                        // Get category
                        var category = categoryChannels.FirstOrDefault(c => c.Id == channel.ParentId)?.Name;

                        // Create channel view model
                        var channelViewModel = _viewModelFactory.CreateChannelViewModel(channel, category);

                        // Add to list
                        channelViewModels.Add(channelViewModel);
                    }

                    // Create guild view model
                    var guildViewModel = _viewModelFactory.CreateGuildViewModel(guild,
                        channelViewModels.OrderBy(c => c.Category)
                            .ThenBy(c => c.Model!.Name)
                            .ToArray());

                    // Add to list
                    availableGuilds.Add(guildViewModel);
                }

                AvailableGuilds = availableGuilds;
                SelectedGuild = AvailableGuilds.FirstOrDefault();
            }
            catch (DiscordChatExporterException ex) when (!ex.IsCritical)
            {
                Notifications.Enqueue(ex.Message.TrimEnd('.'));
            }
        }

        public bool CanExportChannels =>
            !IsBusy && SelectedGuild != null && SelectedChannels != null && SelectedChannels.Any();

        public async void ExportChannels()
        {
            var token = _settingsService.LastToken;
            if (token == null || SelectedGuild == null || SelectedChannels == null || !SelectedChannels.Any())
                return;

            var dialog = _viewModelFactory.CreateExportSetupViewModel(SelectedGuild, SelectedChannels);
            if (await _dialogManager.ShowDialogAsync(dialog) != true)
                return;

            var exporter = new ChannelExporter(token);

            var operations = ProgressManager.CreateOperations(dialog.Channels!.Count);

            var successfulExportCount = 0;
            await dialog.Channels.Zip(operations).ParallelForEachAsync(async tuple =>
            {
                var (channel, operation) = tuple;

                try
                {
                    await exporter.ExportAsync(dialog.Guild!, channel!,
                        dialog.OutputPath!, dialog.SelectedFormat, _settingsService.DateFormat,
                        dialog.PartitionLimit, dialog.After, dialog.Before, operation);

                    Interlocked.Increment(ref successfulExportCount);
                }
                catch (DiscordChatExporterException ex) when (!ex.IsCritical)
                {
                    Notifications.Enqueue(ex.Message.TrimEnd('.'));
                }
                finally
                {
                    operation.Dispose();
                }
            }, _settingsService.ParallelLimit.ClampMin(1));

            // Notify of overall completion
            if (successfulExportCount > 0)
                Notifications.Enqueue($"Successfully exported {successfulExportCount} channel(s)");
        }
    }
}