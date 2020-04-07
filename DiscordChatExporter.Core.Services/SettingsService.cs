﻿using DiscordChatExporter.Core.Models;
using Tyrrrz.Settings;

namespace DiscordChatExporter.Core.Services
{
    public class SettingsService : SettingsManager
    {
        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";

        public bool IsUseUtcEnabled { get; set; } = false;

        public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsTokenPersisted { get; set; } = true;

        public int ParallelLimit { get; set; } = 1;

        public AuthToken? LastToken { get; set; }

        public ExportFormat LastExportFormat { get; set; } = ExportFormat.HtmlDark;

        public int? LastPartitionLimit { get; set; }

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }

        public bool ShouldSerializeLastToken() => IsTokenPersisted;
    }
}