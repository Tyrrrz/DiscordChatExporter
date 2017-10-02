using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscordChatExporter.Messages;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.ViewModels
{
    public class ExportSetupViewModel : ViewModelBase, IExportSetupViewModel
    {
        private readonly ISettingsService _settingsService;

        private string _filePath;
        private ExportFormat _format;
        private DateTime? _from;
        private DateTime? _to;

        public Guild Guild { get; private set; }

        public Channel Channel { get; private set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                Set(ref _filePath, value);
                ExportCommand.RaiseCanExecuteChanged();
            }
        }

        public IReadOnlyList<ExportFormat> AvailableFormats { get; }

        public ExportFormat SelectedFormat
        {
            get => _format;
            set
            {
                Set(ref _format, value);

                // Replace extension in path
                var newExt = value.GetFileExtension();
                if (FilePath != null && !FilePath.EndsWith(newExt))
                    FilePath = FilePath.SubstringUntilLast(".") + "." + newExt;
            }
        }

        public DateTime? From
        {
            get => _from;
            set => Set(ref _from, value);
        }

        public DateTime? To
        {
            get => _to;
            set => Set(ref _to, value);
        }

        // Commands
        public RelayCommand ExportCommand { get; }

        public ExportSetupViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // Defaults
            AvailableFormats = Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

            // Commands
            ExportCommand = new RelayCommand(Export, () => FilePath.IsNotBlank());

            // Messages
            MessengerInstance.Register<ShowExportSetupMessage>(this, m =>
            {
                Guild = m.Guild;
                Channel = m.Channel;
                SelectedFormat = _settingsService.LastExportFormat;
                FilePath = $"{Guild} - {Channel}.{SelectedFormat.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
                From = null;
                To = null;
            });
        }

        private void Export()
        {
            // Save format
            _settingsService.LastExportFormat = SelectedFormat;

            // Start export
            MessengerInstance.Send(new StartExportMessage(Channel, FilePath, SelectedFormat, From, To));
        }
    }
}