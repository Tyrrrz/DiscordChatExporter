using System;
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

    public bool ShouldShowThreads
    {
        get => _settingsService.ShouldShowThreads;
        set => _settingsService.ShouldShowThreads = value;
    }

    public bool ShouldShowArchivedThreads
    {
        get => _settingsService.ShouldShowArchivedThreads;
        set => _settingsService.ShouldShowArchivedThreads = value;
    }

    public string DateFormat
    {
        get => _settingsService.DateFormat;
        set => _settingsService.DateFormat = value;
    }

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    public SettingsViewModel(SettingsService settingsService) => _settingsService = settingsService;
}
