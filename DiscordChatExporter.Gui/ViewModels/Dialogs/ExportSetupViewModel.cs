using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Utils.Extensions;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs
{
    public class ExportSetupViewModel : DialogScreen
    {
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;

        public Guild? Guild { get; set; }

        public IReadOnlyList<Channel>? Channels { get; set; }

        public bool IsSingleChannel => Channels is null || Channels.Count == 1;

        public string? OutputPath { get; set; }

        public IReadOnlyList<ExportFormat> AvailableFormats =>
            Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

        public ExportFormat SelectedFormat { get; set; }

        // This date/time abomination is required because we use separate controls to set these

        public DateTimeOffset? AfterDate { get; set; }

        public bool IsAfterDateSet => AfterDate is not null;

        public TimeSpan? AfterTime { get; set; }

        public DateTimeOffset? After => AfterDate?.Add(AfterTime ?? TimeSpan.Zero);

        public DateTimeOffset? BeforeDate { get; set; }

        public bool IsBeforeDateSet => BeforeDate is not null;

        public TimeSpan? BeforeTime { get; set; }

        public DateTimeOffset? Before => BeforeDate?.Add(BeforeTime ?? TimeSpan.Zero);

        public string? PartitionLimitValue { get; set; }

        public bool ShouldDownloadMedia { get; set; }

        // Whether to show the "advanced options" by default when the dialog opens.
        // This is active if any of the advanced options are set to non-default values.
        public bool IsAdvancedSectionDisplayedByDefault =>
            After != default ||
            Before != default ||
            !string.IsNullOrWhiteSpace(PartitionLimitValue) ||
            ShouldDownloadMedia != default;

        public ExportSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
        {
            _dialogManager = dialogManager;
            _settingsService = settingsService;

            // Persist preferences
            SelectedFormat = _settingsService.LastExportFormat;
            PartitionLimitValue = _settingsService.LastPartitionLimitValue;
            ShouldDownloadMedia = _settingsService.LastShouldDownloadMedia;
        }

        public void Confirm()
        {
            // Persist preferences
            _settingsService.LastExportFormat = SelectedFormat;
            _settingsService.LastPartitionLimitValue = PartitionLimitValue;
            _settingsService.LastShouldDownloadMedia = ShouldDownloadMedia;

            // If single channel - prompt file path
            if (Channels is not null && IsSingleChannel)
            {
                var channel = Channels.Single();
                var defaultFileName = ExportRequest.GetDefaultOutputFileName(
                    Guild!,
                    channel,
                    SelectedFormat,
                    After?.Pipe(Snowflake.FromDate),
                    Before?.Pipe(Snowflake.FromDate)
                );

                // Filter
                var ext = SelectedFormat.GetFileExtension();
                var filter = $"{ext.ToUpperInvariant()} files|*.{ext}";

                OutputPath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);
            }
            // If multiple channels - prompt dir path
            else
            {
                OutputPath = _dialogManager.PromptDirectoryPath();
            }

            if (string.IsNullOrWhiteSpace(OutputPath))
                return;

            Close(true);
        }
    }

    public static class ExportSetupViewModelExtensions
    {
        public static ExportSetupViewModel CreateExportSetupViewModel(this IViewModelFactory factory,
            Guild guild, IReadOnlyList<Channel> channels)
        {
            var viewModel = factory.CreateExportSetupViewModel();

            viewModel.Guild = guild;
            viewModel.Channels = channels;

            return viewModel;
        }
    }
}