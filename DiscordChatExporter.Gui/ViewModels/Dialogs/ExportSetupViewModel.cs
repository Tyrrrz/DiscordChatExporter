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
                // Get single channel
                var channel = Channels.Single();

                // Generate default file name
                var defaultFileName = ChannelExporter.GetDefaultExportFileName(Guild!, channel, SelectedFormat, After, Before);

                // Generate filter
                var ext = SelectedFormat.GetFileExtension();
                var filter = $"{ext.ToUpperInvariant()} files|*.{ext}";

                // Prompt user
                OutputPath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);
            }
            // If multiple channels - prompt dir path
            else
            {
                // Prompt user
                OutputPath = _dialogManager.PromptDirectoryPath();
            }

            // If canceled - return
            if (string.IsNullOrWhiteSpace(OutputPath))
                return;

            // Close dialog
            Close(true);
        }
    }
}