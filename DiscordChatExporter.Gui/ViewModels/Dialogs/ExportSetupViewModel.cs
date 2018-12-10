using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Helpers;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs
{
    public class ExportSetupViewModel : DialogScreen
    {
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;

        public Guild Guild { get; set; }

        public Channel Channel { get; set; }

        public string FilePath { get; set; }

        public IReadOnlyList<ExportFormat> AvailableFormats =>
            Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

        public ExportFormat SelectedFormat { get; set; } = ExportFormat.HtmlDark;

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public int? PartitionLimit { get; set; }

        public ExportSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
        {
            _dialogManager = dialogManager;
            _settingsService = settingsService;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Persist preferences
            SelectedFormat = _settingsService.LastExportFormat;
            PartitionLimit = _settingsService.LastPartitionLimit;
        }

        public void Confirm()
        {
            // Persist preferences
            _settingsService.LastExportFormat = SelectedFormat;
            _settingsService.LastPartitionLimit = PartitionLimit;

            // Clamp 'from' and 'to' values
            if (From > To)
                From = To;
            if (To < From)
                To = From;

            // Generate default file name
            var defaultFileName = ExportHelper.GetDefaultExportFileName(SelectedFormat, Guild, Channel, From, To);

            // Prompt for output file path
            var ext = SelectedFormat.GetFileExtension();
            var filter = $"{ext.ToUpperInvariant()} files|*.{ext}";
            FilePath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);

            // If canceled - return
            if (FilePath.IsBlank())
                return;
            
            // Close dialog
            Close(true);
        }
    }
}