using System.Threading.Tasks;
using CliFx;
using DiscordChatExporter.Core.Services;
using StyletIoC;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static IContainer BuildContainer()
        {
            var builder = new StyletIoCBuilder();

            // Autobind the .Services assembly
            builder.Autobind(typeof(DataService).Assembly);

            // Bind settings as singleton
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();

            // Set instance
            return builder.BuildContainer();
        }

        public static Task<int> Main(string[] args)
        {
            var container = BuildContainer();

            return new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseCommandFactory(schema => (ICommand) container.Get(schema.Type))
                .Build()
                .RunAsync(args);
        }
    }
}