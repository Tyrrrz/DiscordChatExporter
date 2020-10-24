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
using DiscordChatExporter.Gui.Internal;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels.Dialogs;
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

        private IReadOnlyDictionary<Guild, IReadOnlyList<Channel>>? GuildChannelMap { get; set; }

        public IReadOnlyList<Guild>? AvailableGuilds => GuildChannelMap?.Keys.ToArray();

        public Guild? SelectedGuild { get; set; }

        public IReadOnlyList<Channel>? AvailableChannels => SelectedGuild != null
            ? GuildChannelMap?[SelectedGuild]
            : null;

        public IReadOnlyList<Channel>? SelectedChannels { get; set; }

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
                (sender, args) => IsBusy = ProgressManager.IsActive
            );

            ProgressManager.Bind(o => o.IsActive,
                (sender, args) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
            );

            ProgressManager.Bind(o => o.Progress,
                (sender, args) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
            );
        }

        private async ValueTask CheckForUpdatesAsync()
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

            if (_settingsService.IsDarkModeEnabled)
            {
                App.SetDarkTheme();
            }
            else
            {
                App.SetLightTheme();
            }

            await CheckForUpdatesAsync();
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

        public void ShowHelp() => ProcessEx.StartShellExecute(App.GitHubProjectWikiUrl);

        public bool CanPopulateGuildsAndChannels =>
            !IsBusy && !string.IsNullOrWhiteSpace(TokenValue);

        public async void PopulateGuildsAndChannels()
        {
            using var operation = ProgressManager.CreateOperation();

            try
            {
                var tokenValue = TokenValue?.Trim('"', ' ');
                if (string.IsNullOrWhiteSpace(tokenValue))
                    return;

                var token = new AuthToken(
                    IsBotToken ? AuthTokenType.Bot : AuthTokenType.User,
                    tokenValue
                );

                _settingsService.LastToken = token;

                var discord = new DiscordClient(token);

                var guildChannelMap = new Dictionary<Guild, IReadOnlyList<Channel>>();
                await foreach (var guild in discord.GetUserGuildsAsync())
                {
                    guildChannelMap[guild] = await discord.GetGuildChannelsAsync(guild.Id);
                }

                GuildChannelMap = guildChannelMap;
                SelectedGuild = guildChannelMap.Keys.FirstOrDefault();
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
                    var request = new ExportRequest(
                        dialog.Guild!,
                        channel!,
                        dialog.OutputPath!,
                        new[] { dialog.SelectedFormat },
                        dialog.After,
                        dialog.Before,
                        dialog.PartitionLimit,
                        dialog.ShouldDownloadMedia,
                        _settingsService.ShouldReuseMedia,
                        _settingsService.DateFormat
                    );

                    await exporter.ExportChannelAsync(request, operation);

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