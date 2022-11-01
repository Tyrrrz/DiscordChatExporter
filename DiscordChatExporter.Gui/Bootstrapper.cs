using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.ViewModels;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Stylet;
using StyletIoC;

#if !DEBUG
using System.Windows;
using System.Windows.Threading;
#endif

namespace DiscordChatExporter.Gui;

public class Bootstrapper : Bootstrapper<RootViewModel>
{
    protected override void OnStart()
    {
        base.OnStart();

        // Set default theme
        // (preferred theme will be set later, once the settings are loaded)
        App.SetLightTheme();
    }

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);

        // Bind settings as singleton
        builder.Bind<SettingsService>().ToSelf().InSingletonScope();

        // Bind view model factory
        builder.Bind<IViewModelFactory>().ToAbstractFactory();
    }

#if !DEBUG
    protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
    {
        base.OnUnhandledException(e);

        MessageBox.Show(
            e.Exception.ToString(),
            "Error occured",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
#endif
}