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
            SimpleIoc.Default.Register<IExportService, HtmlExportService>();

            // View models
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();
        }

        public static void Cleanup()
        {
        }

        public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();
    }
}