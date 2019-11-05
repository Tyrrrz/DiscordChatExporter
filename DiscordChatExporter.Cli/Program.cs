using System;
using System.Threading.Tasks;
using CliFx;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<DataService>();
            services.AddSingleton<ExportService>();
            services.AddSingleton<SettingsService>();

            // Register commands
            services.AddTransient<ExportChannelCommand>();
            services.AddTransient<ExportDirectMessagesCommand>();
            services.AddTransient<ExportGuildCommand>();
            services.AddTransient<GetChannelsCommand>();
            services.AddTransient<GetDirectMessageChannelsCommand>();
            services.AddTransient<GetGuildsCommand>();
            services.AddTransient<GuideCommand>();

            return services.BuildServiceProvider();
        }

        public static Task<int> Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            return new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseCommandFactory(schema => (ICommand) serviceProvider.GetService(schema.Type))
                .Build()
                .RunAsync(args);
        }
    }
}