using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiscordChatExporter.Gui.Models;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public class SettingsViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsDarkModeEnabled
    {
        get => _settingsService.IsDarkModeEnabled;
        set => _settingsService.IsDarkModeEnabled = value;
    }

    public bool IsTokenPersisted
    {
        get => _settingsService.IsTokenPersisted;
        set => _settingsService.IsTokenPersisted = value;
    }

    public IReadOnlyList<ThreadInclusionMode> AvailableThreadInclusions { get; } =
        Enum.GetValues<ThreadInclusionMode>();

    public ThreadInclusionMode ThreadInclusionMode
    {
        get => _settingsService.ThreadInclusionMode;
        set => _settingsService.ThreadInclusionMode = value;
    }

    public IReadOnlyList<string> AvailableLocales { get; } = new[]
        {
            // Current locale
            CultureInfo.CurrentCulture.Name,
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
        }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    public string Locale
    {
        get => _settingsService.Locale;
        set => _settingsService.Locale = value;
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

    public SettingsViewModel(SettingsService settingsService) => _settingsService = settingsService;
}
