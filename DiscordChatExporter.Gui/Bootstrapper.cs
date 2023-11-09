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

        // Set the default theme.
        // Preferred theme will be set later, once the settings are loaded.
        App.SetLightTheme();
    }

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);

        builder.Bind<SettingsService>().ToSelf().InSingletonScope();
        builder.Bind<IViewModelFactory>().ToAbstractFactory();
    }

#if !DEBUG
    protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
    {
        base.OnUnhandledException(args);

        MessageBox.Show(
            args.Exception.ToString(),
            "Error occured",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
#endif
}
