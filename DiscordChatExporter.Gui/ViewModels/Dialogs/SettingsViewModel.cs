using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Utils.Extensions;
using DiscordChatExporter.Gui.Models;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels.Framework;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public class SettingsViewModel(SettingsService settingsService) : DialogScreen
{
    public bool IsAutoUpdateEnabled
    {
        get => settingsService.IsAutoUpdateEnabled;
        set => settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsDarkModeEnabled
    {
        get => settingsService.IsDarkModeEnabled;
        set => settingsService.IsDarkModeEnabled = value;
    }

    public bool IsTokenPersisted
    {
        get => settingsService.IsTokenPersisted;
        set => settingsService.IsTokenPersisted = value;
    }

    public IReadOnlyList<ThreadInclusionMode> AvailableThreadInclusions { get; } =
        Enum.GetValues<ThreadInclusionMode>();

    public ThreadInclusionMode ThreadInclusionMode
    {
        get => settingsService.ThreadInclusionMode;
        set => settingsService.ThreadInclusionMode = value;
    }

    // These items have to be non-nullable because WPF ComboBox doesn't allow a null value to be selected
    public IReadOnlyList<string> AvailableLocales { get; } = new[]
        {
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
        }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    // This has to be non-nullable because WPF ComboBox doesn't allow a null value to be selected
    public string Locale
    {
        get => settingsService.Locale ?? "";
        // Important to reduce empty strings to nulls, because empty strings don't correspond to valid cultures
        set => settingsService.Locale = value.NullIfWhiteSpace();
    }

    public bool IsUtcNormalizationEnabled
    {
        get => settingsService.IsUtcNormalizationEnabled;
        set => settingsService.IsUtcNormalizationEnabled = value;
    }

    public int ParallelLimit
    {
        get => settingsService.ParallelLimit;
        set => settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }
}
