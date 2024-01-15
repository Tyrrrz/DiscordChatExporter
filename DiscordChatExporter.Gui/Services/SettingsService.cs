using System;
using System.IO;
using Cogwheel;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Gui.Models;
using Microsoft.Win32;

namespace DiscordChatExporter.Gui.Services;

public partial class SettingsService()
    : SettingsBase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"))
{
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();

    public bool IsTokenPersisted { get; set; } = true;

    public ThreadInclusionMode ThreadInclusionMode { get; set; } = ThreadInclusionMode.None;

    public string? Locale { get; set; }

    public bool IsUtcNormalizationEnabled { get; set; }

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
            return Registry
                .CurrentUser.OpenSubKey(
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
