using CommonServiceLocator;
using DiscordChatExporter.Core.Services;
using GalaSoft.MvvmLight.Ioc;

namespace DiscordChatExporter.Cli
{
    public class Container
    {
        public IDataService DataService => Resolve<IDataService>();
        public IExportService ExportService => Resolve<IExportService>();
        public IMessageGroupService MessageGroupService => Resolve<IMessageGroupService>();
        public ISettingsService SettingsService => Resolve<ISettingsService>();
        public IUpdateService UpdateService => Resolve<IUpdateService>();

        public Container()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();

            // Services
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<IMessageGroupService, MessageGroupService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUpdateService, UpdateService>();
        }

        private T Resolve<T>(string key = null)
        {
            return ServiceLocator.Current.GetInstance<T>(key);
        }
    }
}