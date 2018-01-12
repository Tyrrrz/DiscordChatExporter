using DiscordChatExporter.Cli.ViewModels;
using DiscordChatExporter.Core.Services;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DiscordChatExporter.Cli
{
    public class Container
    {
        public IMainViewModel MainViewModel => Resolve<IMainViewModel>();
        public ISettingsService SettingsService => Resolve<ISettingsService>();

        private T Resolve<T>(string key = null)
        {
            return ServiceLocator.Current.GetInstance<T>(key);
        }

        public void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();

            // Services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<IMessageGroupService, MessageGroupService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();

            // View models
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
        }

        public void Cleanup()
        {
        }
    }
}