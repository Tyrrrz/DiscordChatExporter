﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels
{
    public class ExportSetupViewModel : ViewModelBase, IExportSetupViewModel
    {
        private readonly ISettingsService _settingsService;

        private string _filePath;
        private ExportFormat _format;
        private DateTime? _from;
        private DateTime? _to;
        private int? _partitionLimit;

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

        public IReadOnlyList<ExportFormat> AvailableFormats =>
            Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

        public ExportFormat SelectedFormat
        {
            get => _format;
            set
            {
                Set(ref _format, value);

                // Replace extension in path
                var ext = value.GetFileExtension();
                if (FilePath != null)
                    FilePath = Path.ChangeExtension(FilePath, ext);
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

        public int? PartitionLimit
        {
            get => _partitionLimit;
            set => Set(ref _partitionLimit, value);
        }

        // Commands
        public RelayCommand ExportCommand { get; }

        public ExportSetupViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // Commands
            ExportCommand = new RelayCommand(Export, () => FilePath.IsNotBlank());

            // Messages
            MessengerInstance.Register<ShowExportSetupMessage>(this, m =>
            {
                Guild = m.Guild;
                Channel = m.Channel;
                SelectedFormat = _settingsService.LastExportFormat;
                FilePath = $"{Guild.Name} - {Channel.Name}.{SelectedFormat.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
                From = null;
                To = null;
                PartitionLimit = _settingsService.LastPartitionLimit;
            });
        }

        private void Export()
        {
            // Persist preferences
            _settingsService.LastExportFormat = SelectedFormat;
            _settingsService.LastPartitionLimit = PartitionLimit;

            // Clamp 'from' and 'to' values
            if (From > To)
                From = To;
            if (To < From)
                To = From;

            // Start export
            MessengerInstance.Send(new StartExportMessage(Channel, FilePath, SelectedFormat, From, To, PartitionLimit));
        }
    }
}