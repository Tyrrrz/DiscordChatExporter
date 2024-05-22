using System;
using System.IO;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Models;

namespace DiscordChatExporter.Gui.Services;

// Can't use [ObservableProperty] here because System.Text.Json's source generator doesn't see
// the generated properties.
[INotifyPropertyChanged]
public partial class SettingsService()
    : SettingsBase(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"),
        SerializerContext.Default
    )
{
    private bool _isUkraineSupportMessageEnabled = true;
    public bool IsUkraineSupportMessageEnabled
    {
        get => _isUkraineSupportMessageEnabled;
        set => SetProperty(ref _isUkraineSupportMessageEnabled, value);
    }

    private ThemeVariant _theme;
    public ThemeVariant Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    private bool _isAutoUpdateEnabled = true;
    public bool IsAutoUpdateEnabled
    {
        get => _isAutoUpdateEnabled;
        set => SetProperty(ref _isAutoUpdateEnabled, value);
    }

    private bool _isTokenPersisted = true;
    public bool IsTokenPersisted
    {
        get => _isTokenPersisted;
        set => SetProperty(ref _isTokenPersisted, value);
    }

    private ThreadInclusionMode _threadInclusionMode;
    public ThreadInclusionMode ThreadInclusionMode
    {
        get => _threadInclusionMode;
        set => SetProperty(ref _threadInclusionMode, value);
    }

    private string? _locale;
    public string? Locale
    {
        get => _locale;
        set => SetProperty(ref _locale, value);
    }

    private bool _isUtcNormalizationEnabled;
    public bool IsUtcNormalizationEnabled
    {
        get => _isUtcNormalizationEnabled;
        set => SetProperty(ref _isUtcNormalizationEnabled, value);
    }

    private int _parallelLimit = 1;
    public int ParallelLimit
    {
        get => _parallelLimit;
        set => SetProperty(ref _parallelLimit, value);
    }

    private string? _lastToken;
    public string? LastToken
    {
        get => _lastToken;
        set => SetProperty(ref _lastToken, value);
    }

    private ExportFormat _lastExportFormat = ExportFormat.HtmlDark;
    public ExportFormat LastExportFormat
    {
        get => _lastExportFormat;
        set => SetProperty(ref _lastExportFormat, value);
    }

    private string? _lastPartitionLimitValue;
    public string? LastPartitionLimitValue
    {
        get => _lastPartitionLimitValue;
        set => SetProperty(ref _lastPartitionLimitValue, value);
    }

    private string? _lastMessageFilterValue;
    public string? LastMessageFilterValue
    {
        get => _lastMessageFilterValue;
        set => SetProperty(ref _lastMessageFilterValue, value);
    }

    private bool _lastShouldFormatMarkdown = true;
    public bool LastShouldFormatMarkdown
    {
        get => _lastShouldFormatMarkdown;
        set => SetProperty(ref _lastShouldFormatMarkdown, value);
    }

    private bool _lastShouldDownloadAssets;
    public bool LastShouldDownloadAssets
    {
        get => _lastShouldDownloadAssets;
        set => SetProperty(ref _lastShouldDownloadAssets, value);
    }

    private bool _lastShouldReuseAssets;
    public bool LastShouldReuseAssets
    {
        get => _lastShouldReuseAssets;
        set => SetProperty(ref _lastShouldReuseAssets, value);
    }

    private string? _lastAssetsDirPath;
    public string? LastAssetsDirPath
    {
        get => _lastAssetsDirPath;
        set => SetProperty(ref _lastAssetsDirPath, value);
    }

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
    [JsonSerializable(typeof(SettingsService))]
    private partial class SerializerContext : JsonSerializerContext;
}
