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

        public static async Task<int> Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(serviceProvider.GetService)
                .Build()
                .RunAsync(args);
        }
    }
}