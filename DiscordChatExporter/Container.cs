using DiscordChatExporter.Services;
using DiscordChatExporter.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DiscordChatExporter
{
    public class Container
    {
        public ICliViewModel CliViewModel => Resolve<ICliViewModel>();
        public IErrorViewModel ErrorViewModel => Resolve<IErrorViewModel>();
        public IExportDoneViewModel ExportDoneViewModel => Resolve<IExportDoneViewModel>();
        public IExportSetupViewModel ExportSetupViewModel => Resolve<IExportSetupViewModel>();
        public IMainViewModel MainViewModel => Resolve<IMainViewModel>();
        public ISettingsViewModel SettingsViewModel => Resolve<ISettingsViewModel>();

        private T Resolve<T>(string key = null)
        {
            return ServiceLocator.Current.GetInstance<T>(key);
        }

        public void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();

            // Settings
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            Resolve<ISettingsService>().Load();

            // Services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<IMessageGroupService, MessageGroupService>();

            // View models
            SimpleIoc.Default.Register<IErrorViewModel, ErrorViewModel>(true);
            SimpleIoc.Default.Register<IExportDoneViewModel, ExportDoneViewModel>(true);
            SimpleIoc.Default.Register<IExportSetupViewModel, ExportSetupViewModel>(true);
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
            SimpleIoc.Default.Register<ISettingsViewModel, SettingsViewModel>(true);
        }

        public void Cleanup()
        {
            // Settings
            ServiceLocator.Current.GetInstance<ISettingsService>().Save();
        }
    }
}