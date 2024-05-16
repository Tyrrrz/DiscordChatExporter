﻿using System;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.Utils;
using DiscordChatExporter.Gui.Utils.Extensions;
using DiscordChatExporter.Gui.ViewModels;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Dialogs;
using DiscordChatExporter.Gui.Views;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChatExporter.Gui;

public class App : Application, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();

    private readonly ServiceProvider _services;
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    public App()
    {
        var services = new ServiceCollection();

        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ExportSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();

        _services = services.BuildServiceProvider(true);
        _settingsService = _services.GetRequiredService<SettingsService>();
        _mainViewModel = _services.GetRequiredService<ViewModelManager>().CreateMainViewModel();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                () =>
                {
                    RequestedThemeVariant = _settingsService.Theme switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default
                    };

                    InitializeTheme();
                },
                false
            )
        );
    }

    public override void Initialize()
    {
        base.Initialize();

        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;

        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeTheme()
    {
        var actualTheme = RequestedThemeVariant?.Key switch
        {
            "Light" => PlatformThemeVariant.Light,
            "Dark" => PlatformThemeVariant.Dark,
            _ => PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light
        };

        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme =
            actualTheme == PlatformThemeVariant.Light
                ? Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"))
                : Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set up custom theme colors
        InitializeTheme();

        // Load settings
        _settingsService.Load();
    }

    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    public void Dispose()
    {
        _eventRoot.Dispose();
        _services.Dispose();
    }
}
