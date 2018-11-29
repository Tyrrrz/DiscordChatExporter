using System.Windows;
using System.Windows.Threading;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Stylet;
using StyletIoC;

namespace DiscordChatExporter.Gui
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            // Autobind services in the .Core assembly
            builder.Autobind(typeof(DataService).Assembly);

            // Bind settings as singleton
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();

            // Bind view model factory
            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(e);

            MessageBox.Show(e.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}