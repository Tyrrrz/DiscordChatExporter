using DiscordChatExporter.Services;
using DiscordChatExporter.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DiscordChatExporter
{
    public class Locator
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Services
            SimpleIoc.Default.Register<IApiService, ApiService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();

            // View models
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();

            // Load settings
            ServiceLocator.Current.GetInstance<ISettingsService>().Load();
        }

        public static void Cleanup()
        {
            // Save settings
            ServiceLocator.Current.GetInstance<ISettingsService>().Save();
        }

        public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();
    }
}