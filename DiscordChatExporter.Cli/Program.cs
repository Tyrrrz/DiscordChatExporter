using System;
using System.Reflection;
using DiscordChatExporter.Core.Models;
using Fclp;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static readonly Container Container = new Container();

        private static void ShowHelp()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var availableFormats = Enum.GetNames(typeof(ExportFormat));

            Console.WriteLine($"=== Discord Chat Exporter (Command Line Interface) v{version} ===");
            Console.WriteLine();
            Console.WriteLine("[-t] [--token]       Discord authorization token.");
            Console.WriteLine("[-b] [--bot]         Whether this is a bot token.");
            Console.WriteLine("[-c] [--channel]     Discord channel ID.");
            Console.WriteLine("[-f] [--format]      Export format. Optional.");
            Console.WriteLine("[-o] [--output]      Output file path. Optional.");
            Console.WriteLine("     [--datefrom]    Limit to messages after this date. Optional.");
            Console.WriteLine("     [--dateto]      Limit to messages before this date. Optional.");
            Console.WriteLine("     [--dateformat]  Date format. Optional.");
            Console.WriteLine("     [--grouplimit]  Message group limit. Optional.");
            Console.WriteLine();
            Console.WriteLine($"Available export formats: {availableFormats.JoinToString(", ")}");
            Console.WriteLine();
            Console.WriteLine("# To get user token:");
            Console.WriteLine(" - Open Discord app");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" - Navigate to the Application tab");
            Console.WriteLine(" - Expand Storage > Local Storage > https://discordapp.com");
            Console.WriteLine(" - Find the \"token\" key and copy its value");
            Console.WriteLine();
            Console.WriteLine("# To get bot token:");
            Console.WriteLine(" - Go to Discord developer portal");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Open your application's settings");
            Console.WriteLine(" - Navigate to the Bot section on the left");
            Console.WriteLine(" - Under Token click Copy");
            Console.WriteLine();
            Console.WriteLine("# To get channel ID:");
            Console.WriteLine(" - Open Discord app");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Go to any channel you want to export");
            Console.WriteLine(" - Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" - Navigate to the Console tab");
            Console.WriteLine(" - Type \"document.URL\" and press Enter");
            Console.WriteLine(" - Copy the long sequence of numbers after last slash");
        }

        private static CliOptions ParseOptions(string[] args)
        {
            var argsParser = new FluentCommandLineParser<CliOptions>();

            var settings = Container.SettingsService;

            argsParser.Setup(o => o.TokenValue).As('t', "token").Required();
            argsParser.Setup(o => o.IsBotToken).As('b', "bot").SetDefault(false);
            argsParser.Setup(o => o.ChannelId).As('c', "channel").Required();
            argsParser.Setup(o => o.ExportFormat).As('f', "format").SetDefault(ExportFormat.HtmlDark);
            argsParser.Setup(o => o.FilePath).As('o', "output").SetDefault(null);
            argsParser.Setup(o => o.From).As("datefrom").SetDefault(null);
            argsParser.Setup(o => o.To).As("dateto").SetDefault(null);
            argsParser.Setup(o => o.DateFormat).As("dateformat").SetDefault(settings.DateFormat);
            argsParser.Setup(o => o.MessageGroupLimit).As("grouplimit").SetDefault(settings.MessageGroupLimit);

            var parsed = argsParser.Parse(args);

            // Show help if no arguments
            if (parsed.EmptyArgs)
            {
                ShowHelp();
                Environment.Exit(0);
            }
            // Show error if there are any
            else if (parsed.HasErrors)
            {
                Console.Error.Write(parsed.ErrorText);
                Environment.Exit(-1);
            }

            return argsParser.Object;
        }

        public static void Main(string[] args)
        {
            // Init container
            Container.Init();

            // Parse options
            var options = ParseOptions(args);

            // Inject some settings
            var settings = Container.SettingsService;
            settings.DateFormat = options.DateFormat;
            settings.MessageGroupLimit = options.MessageGroupLimit;

            // Create token
            var token = new AuthToken(
                options.IsBotToken ? AuthTokenType.Bot : AuthTokenType.User,
                options.TokenValue);

            // Export
            var vm = Container.MainViewModel;
            vm.ExportAsync(
                token,
                options.ChannelId,
                options.FilePath,
                options.ExportFormat,
                options.From,
                options.To).GetAwaiter().GetResult();

            // Cleanup container
            Container.Cleanup();

            Console.WriteLine("Export complete.");
        }
    }
}