using System;
using System.IO;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Models;

namespace DiscordChatExporter.Gui.Services;

[INotifyPropertyChanged]
public partial class SettingsService()
    : SettingsBase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"))
{
    [ObservableProperty]
    private bool _isUkraineSupportMessageEnabled = true;

    [ObservableProperty]
    private ThemeVariant _theme;

    [ObservableProperty]
    private bool _isAutoUpdateEnabled = true;

    [ObservableProperty]
    private bool _isTokenPersisted = true;

    [ObservableProperty]
    private ThreadInclusionMode _threadInclusionMode;

    [ObservableProperty]
    private string? _locale;

    [ObservableProperty]
    private bool _isUtcNormalizationEnabled;

    [ObservableProperty]
    private int _parallelLimit = 1;

    [ObservableProperty]
    private string? _lastToken;

    [ObservableProperty]
    private ExportFormat _lastExportFormat = ExportFormat.HtmlDark;

    [ObservableProperty]
    private string? _lastPartitionLimitValue;

    [ObservableProperty]
    private string? _lastMessageFilterValue;

    [ObservableProperty]
    private bool _lastShouldFormatMarkdown = true;

    [ObservableProperty]
    private bool _lastShouldDownloadAssets;

    [ObservableProperty]
    private bool _lastShouldReuseAssets;

    [ObservableProperty]
    private string? _lastAssetsDirPath;

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
