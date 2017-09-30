using DiscordChatExporter.Services;
using DiscordChatExporter.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DiscordChatExporter
{
    public class Container
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<IMessageGroupService, MessageGroupService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();

            // View models
            SimpleIoc.Default.Register<IErrorViewModel, ErrorViewModel>(true);
            SimpleIoc.Default.Register<IExportDoneViewModel, ExportDoneViewModel>(true);
            SimpleIoc.Default.Register<IExportSetupViewModel, ExportSetupViewModel>(true);
            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
            SimpleIoc.Default.Register<ISettingsViewModel, SettingsViewModel>(true);

            // Load settings
            ServiceLocator.Current.GetInstance<ISettingsService>().Load();
        }

        public static void Cleanup()
        {
            // Save settings
            ServiceLocator.Current.GetInstance<ISettingsService>().Save();
        }

        public IErrorViewModel ErrorViewModel => ServiceLocator.Current.GetInstance<IErrorViewModel>();
        public IExportDoneViewModel ExportDoneViewModel => ServiceLocator.Current.GetInstance<IExportDoneViewModel>();
        public IExportSetupViewModel ExportSetupViewModel => ServiceLocator.Current.GetInstance<IExportSetupViewModel>();
        public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();
        public ISettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<ISettingsViewModel>();
    }
}