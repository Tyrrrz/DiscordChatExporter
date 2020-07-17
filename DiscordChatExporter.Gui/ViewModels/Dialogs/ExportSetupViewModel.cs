using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting;
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

        public bool IsSingleChannel => Channels == null || Channels.Count == 1;

        public string? OutputPath { get; set; }

        public IReadOnlyList<ExportFormat> AvailableFormats =>
            Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

        public ExportFormat SelectedFormat { get; set; }

        public DateTimeOffset? After { get; set; }

        public DateTimeOffset? Before { get; set; }

        public int? PartitionLimit { get; set; }

        public bool IsMediaExported { get; set; }

        // Whether to show the "advanced options" by default when the dialog opens.
        // This is active if any of the advanced options are set to non-default values.
        public bool IsAdvancedSectionDisplayedByDefault =>
            After != default ||
            Before != default ||
            PartitionLimit != default ||
            IsMediaExported != default;

        public ExportSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
        {
            _dialogManager = dialogManager;
            _settingsService = settingsService;

            // Persist preferences
            SelectedFormat = _settingsService.LastExportFormat;
            PartitionLimit = _settingsService.LastPartitionLimit;
            IsMediaExported = _settingsService.LastIsMediaExported;
        }

        public void Confirm()
        {
            // Persist preferences
            _settingsService.LastExportFormat = SelectedFormat;
            _settingsService.LastPartitionLimit = PartitionLimit;
            _settingsService.LastIsMediaExported = IsMediaExported;

            // Clamp 'after' and 'before' values
            if (After > Before)
                After = Before;
            if (Before < After)
                Before = After;

            // If single channel - prompt file path
            if (IsSingleChannel)
            {
                var channel = Channels.Single();
                var defaultFileName = ExportRequest.GetDefaultOutputFileName(Guild!, channel, SelectedFormat, After, Before);

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

            // If canceled - return
            if (string.IsNullOrWhiteSpace(OutputPath))
                return;

            Close(true);
        }
    }
}