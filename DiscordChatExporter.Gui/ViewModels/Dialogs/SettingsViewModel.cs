using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Utils.Extensions;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Models;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.Utils;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public class SettingsViewModel : DialogViewModelBase
{
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _eventRoot.Add(_settingsService.WatchAllProperties(OnAllPropertiesChanged));
    }

    public IReadOnlyList<ThemeVariant> AvailableThemes { get; } = Enum.GetValues<ThemeVariant>();

    public ThemeVariant Theme
    {
        get => _settingsService.Theme;
        set => _settingsService.Theme = value;
    }

    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsTokenPersisted
    {
        get => _settingsService.IsTokenPersisted;
        set => _settingsService.IsTokenPersisted = value;
    }

    public IReadOnlyList<ThreadInclusionMode> AvailableThreadInclusionModes { get; } =
        Enum.GetValues<ThreadInclusionMode>();

    public ThreadInclusionMode ThreadInclusionMode
    {
        get => _settingsService.ThreadInclusionMode;
        set => _settingsService.ThreadInclusionMode = value;
    }

    // These items have to be non-nullable because Avalonia ComboBox doesn't allow a null value to be selected
    public IReadOnlyList<string> AvailableLocales { get; } =
        [
            // Current locale (maps to null downstream)
            "",
            // Locales supported by the Discord app
            "da-DK",
            "de-DE",
            "en-GB",
            "en-US",
            "es-ES",
            "fr-FR",
            "hr-HR",
            "it-IT",
            "lt-LT",
            "hu-HU",
            "nl-NL",
            "no-NO",
            "pl-PL",
            "pt-BR",
            "ro-RO",
            "fi-FI",
            "sv-SE",
            "vi-VN",
            "tr-TR",
            "cs-CZ",
            "el-GR",
            "bg-BG",
            "ru-RU",
            "uk-UA",
            "th-TH",
            "zh-CN",
            "ja-JP",
            "zh-TW",
            "ko-KR"
        ];

    // This has to be non-nullable because Avalonia ComboBox doesn't allow a null value to be selected
    public string Locale
    {
        get => _settingsService.Locale ?? "";
        // Important to reduce empty strings to nulls, because empty strings don't correspond to valid cultures
        set => _settingsService.Locale = value.NullIfWhiteSpace();
    }

    public bool IsUtcNormalizationEnabled
    {
        get => _settingsService.IsUtcNormalizationEnabled;
        set => _settingsService.IsUtcNormalizationEnabled = value;
    }

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
