using System;
using System.IO;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Models;

namespace DiscordChatExporter.Gui.Services;

[ObservableObject]
public partial class SettingsService()
    : SettingsBase(
        Path.Combine(AppContext.BaseDirectory, "Settings.dat"),
        SerializerContext.Default
    )
{
    [ObservableProperty]
    public partial bool IsUkraineSupportMessageEnabled { get; set; } = true;

    [ObservableProperty]
    public partial ThemeVariant Theme { get; set; }

    [ObservableProperty]
    public partial bool IsAutoUpdateEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsTokenPersisted { get; set; } = true;

    [ObservableProperty]
    public partial RateLimitPreference RateLimitPreference { get; set; } =
        RateLimitPreference.RespectAll;

    [ObservableProperty]
    public partial ThreadInclusionMode ThreadInclusionMode { get; set; }

    [ObservableProperty]
    public partial string? Locale { get; set; }

    [ObservableProperty]
    public partial bool IsUtcNormalizationEnabled { get; set; }

    [ObservableProperty]
    public partial int ParallelLimit { get; set; } = 1;

    [ObservableProperty]
    public partial string? LastToken { get; set; }

    [ObservableProperty]
    public partial ExportFormat LastExportFormat { get; set; } = ExportFormat.HtmlDark;

    [ObservableProperty]
    public partial string? LastPartitionLimitValue { get; set; }

    [ObservableProperty]
    public partial string? LastMessageFilterValue { get; set; }

    [ObservableProperty]
    public partial bool LastShouldFormatMarkdown { get; set; } = true;

    [ObservableProperty]
    public partial bool LastShouldDownloadAssets { get; set; }

    [ObservableProperty]
    public partial bool LastShouldReuseAssets { get; set; }

    [ObservableProperty]
    public partial string? LastAssetsDirPath { get; set; }

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
