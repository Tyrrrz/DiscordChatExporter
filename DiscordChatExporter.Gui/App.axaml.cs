using System;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Dialogs;
using DiscordChatExporter.Gui.Views;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChatExporter.Gui;

public partial class App : Application, IDisposable
{
    private readonly ServiceProvider _services;
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
        _mainViewModel = _services.GetRequiredService<ViewModelManager>().CreateMainViewModel();
    }

    public override void Initialize()
    {
        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set custom theme colors
        SetDefaultTheme();
    }

    public void Dispose() => _services.Dispose();
}

public partial class App
{
    public static void SetLightTheme()
    {
        if (Current is null)
            return;

        Current.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme = Theme.Create(
            Theme.Light,
            Color.Parse("#343838"),
            Color.Parse("#F9A825")
        );
    }

    public static void SetDarkTheme()
    {
        if (Current is null)
            return;

        Current.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme = Theme.Create(
            Theme.Dark,
            Color.Parse("#E8E8E8"),
            Color.Parse("#F9A825")
        );
    }

    public static void SetDefaultTheme()
    {
        if (Current is null)
            return;

        var isDarkModeEnabledByDefault =
            Current.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark;

        if (isDarkModeEnabledByDefault)
            SetDarkTheme();
        else
            SetLightTheme();
    }
}
