using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Localization;
using DiscordChatExporter.Gui.Models;

namespace DiscordChatExporter.Gui.Services;

[ObservableObject]
public partial class SettingsService()
    : SettingsBase(StartOptions.Current.SettingsPath, SerializerContext.Default)
{
    [ObservableProperty]
    public partial bool IsUkraineSupportMessageEnabled { get; set; } = true;

    [ObservableProperty]
    public partial ThemeVariant Theme { get; set; }

    [ObservableProperty]
    public partial Language Language { get; set; }

    [ObservableProperty]
    public partial bool IsAutoUpdateEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsTokenPersisted { get; set; }

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
    [JsonConverter(typeof(TokenEncryptionConverter))]
    public partial string? LastToken { get; set; }

    [ObservableProperty]
    public partial ExportFormat LastExportFormat { get; set; } = ExportFormat.Json;

    [ObservableProperty]
    public partial string? LastPartitionLimitValue { get; set; }

    [ObservableProperty]
    public partial string? LastMessageFilterValue { get; set; }

    [ObservableProperty]
    public partial bool LastIsReverseMessageOrder { get; set; }

    [ObservableProperty]
    public partial bool LastShouldFormatMarkdown { get; set; } = true;

    [ObservableProperty]
    public partial bool LastShouldDownloadAssets { get; set; } = true;

    [ObservableProperty]
    public partial bool LastShouldReuseAssets { get; set; } = true;

    [ObservableProperty]
    public partial string? LastAssetsDirPath { get; set; }

    [ObservableProperty]
    public partial bool LastForumShouldDownloadAssets { get; set; } = true;

    [ObservableProperty]
    public partial bool LastForumShouldReuseAssets { get; set; } = true;

    [ObservableProperty]
    public partial string? LastForumAssetsDirPath { get; set; }

    [ObservableProperty]
    public partial bool LastForumShouldDownloadAvatars { get; set; }

    [ObservableProperty]
    public partial ForumCommonAssetMode LastForumCommonAssetMode { get; set; } =
        ForumCommonAssetMode.SharedFolder;

    [ObservableProperty]
    public partial ForumAttachmentFolderMode LastForumAttachmentFolderMode { get; set; } =
        ForumAttachmentFolderMode.ByMediaType;

    [ObservableProperty]
    public partial ForumAttachmentNamingMode LastForumAttachmentNamingMode { get; set; } =
        ForumAttachmentNamingMode.AttachmentIdAndOriginal;

    [ObservableProperty]
    public partial int LastForumParallelLimit { get; set; } = 4;

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
