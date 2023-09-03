using System;
using System.Collections.Generic;
using System.Globalization;
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

    public IReadOnlyList<CultureInfo> AvailableLocales { get; } =
        CultureInfo.GetCultures(CultureTypes.SpecificCultures);

    public CultureInfo Locale
    {
        get => _settingsService.Locale;
        set => _settingsService.Locale = value;
    }

    public TimeSpan UtcOffset
    {
        get => _settingsService.UtcOffset;
        set => _settingsService.UtcOffset = value;
    }

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    public SettingsViewModel(SettingsService settingsService) => _settingsService = settingsService;
}
