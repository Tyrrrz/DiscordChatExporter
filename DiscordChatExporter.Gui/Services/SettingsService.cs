using System;
using System.IO;
using Cogwheel;
using DiscordChatExporter.Core.Exporting;
using Microsoft.Win32;

namespace DiscordChatExporter.Gui.Services;

public partial class SettingsService : SettingsBase
{
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();

    public bool IsTokenPersisted { get; set; } = true;

    public bool ShouldShowThreads { get; set; }

    public bool ShouldShowArchivedThreads { get; set; }

    public string DateFormat { get; set; } = "MM/dd/yyyy h:mm tt";

    public int ParallelLimit { get; set; } = 1;

    public Version? LastAppVersion { get; set; }

    public string? LastToken { get; set; }

    public ExportFormat LastExportFormat { get; set; } = ExportFormat.HtmlDark;

    public string? LastPartitionLimitValue { get; set; }

    public string? LastMessageFilterValue { get; set; }

    public bool LastShouldFormatMarkdown { get; set; } = true;

    public bool LastShouldDownloadAssets { get; set; }

    public bool LastShouldReuseAssets { get; set; }

    public string? LastAssetsDirPath { get; set; }

    public SettingsService()
        : base(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat")) { }

    public override void Save()
    {
        // Clear the token if it's not supposed to be persisted
        var lastToken = LastToken;
        if (!IsTokenPersisted)
            LastToken = null;

        base.Save();

        LastToken = lastToken;
    }
}

public partial class SettingsService
{
    private static bool IsDarkModeEnabledByDefault()
    {
        try
        {
            return Registry.CurrentUser
                .OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                    false
                )
                ?.GetValue("AppsUseLightTheme")
                is 0;
        }
        catch
        {
            return false;
        }
    }
}
