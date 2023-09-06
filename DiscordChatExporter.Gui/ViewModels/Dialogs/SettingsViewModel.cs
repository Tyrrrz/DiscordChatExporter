using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Exporting;
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

    public IReadOnlyList<Locale> AvailableLocales { get; } = new[]
        {
            // Current locale
            Locale.Current,
            // Locales supported by the Discord app
            new Locale("da-DK"),
            new Locale("de-DE"),
            new Locale("en-GB"),
            new Locale("en-US"),
            new Locale("es-ES"),
            new Locale("fr-FR"),
            new Locale("hr-HR"),
            new Locale("it-IT"),
            new Locale("lt-LT"),
            new Locale("hu-HU"),
            new Locale("nl-NL"),
            new Locale("no-NO"),
            new Locale("pl-PL"),
            new Locale("pt-BR"),
            new Locale("ro-RO"),
            new Locale("fi-FI"),
            new Locale("sv-SE"),
            new Locale("vi-VN"),
            new Locale("tr-TR"),
            new Locale("cs-CZ"),
            new Locale("el-GR"),
            new Locale("bg-BG"),
            new Locale("ru-RU"),
            new Locale("uk-UA"),
            new Locale("th-TH"),
            new Locale("zh-CN"),
            new Locale("ja-JP"),
            new Locale("zh-TW"),
            new Locale("ko-KR")
        }.Distinct().ToArray();

    public Locale Locale
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
