using CommonServiceLocator;
using DiscordChatExporter.Core.Services;
using GalaSoft.MvvmLight.Ioc;

namespace DiscordChatExporter.Cli
{
    public class Container
    {
        public Container()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Reset();

            // Services
            SimpleIoc.Default.Register<IChatLogService, ChatLogService>();
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<IExportService, ExportService>();
            SimpleIoc.Default.Register<IMessageGroupService, MessageGroupService>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<IUpdateService, UpdateService>();
        }

        public T Resolve<T>(string key = null)
        {
            return ServiceLocator.Current.GetInstance<T>(key);
        }
    }
}