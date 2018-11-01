using CommonServiceLocator;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Gui.ViewModels;
using GalaSoft.MvvmLight.Ioc;

namespace DiscordChatExporter.Gui
{
    public class Container
    {
        public IExportSetupViewModel ExportSetupViewModel => Resolve<IExportSetupViewModel>();
        public IMainViewModel MainViewModel => Resolve<IMainViewModel>();
        public ISettingsViewModel SettingsViewModel => Resolve<ISettingsViewModel>();

        public Container()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();

            // Services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUpdateService, UpdateService>();

            // View models
            SimpleIoc.Default.Register<IExportSetupViewModel, ExportSetupViewModel>(true);
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
            SimpleIoc.Default.Register<ISettingsViewModel, SettingsViewModel>(true);
        }

        private T Resolve<T>(string key = null)
        {
            return ServiceLocator.Current.GetInstance<T>(key);
        }
    }
}