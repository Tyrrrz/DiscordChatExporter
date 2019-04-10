using DiscordChatExporter.Core.Services;
using StyletIoC;

namespace DiscordChatExporter.Cli
{
    public static class Container
    {
        public static IContainer Instance { get; }

        static Container()
        {
            var builder = new StyletIoCBuilder();

            // Autobind the .Services assembly
            builder.Autobind(typeof(DataService).Assembly);

            // Bind settings as singleton
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();

            // Set instance
            Instance = builder.BuildContainer();
        }
    }
}