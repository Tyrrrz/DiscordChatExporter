using System;
using System.Reflection;
using DiscordChatExporter.Core.Models;
using Fclp;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static readonly Container Container = new Container();

        private static void ShowHelp()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine($"=== Discord Chat Exporter (Command Line Interface) v{version} ===");
            Console.WriteLine();
            Console.WriteLine("[-t] [--token]       Discord authorization token.");
            Console.WriteLine("[-c] [--channel]     Discord channel ID.");
            Console.WriteLine("[-f] [--format]      Export format (PlainText/HtmlDark/HtmlLight). Optional.");
            Console.WriteLine("[-o] [--output]      Output file path. Optional.");
            Console.WriteLine("     [--datefrom]    Limit to messages after this date. Optional.");
            Console.WriteLine("     [--dateto]      Limit to messages before this date. Optional.");
            Console.WriteLine("     [--dateformat]  Date format. Optional.");
            Console.WriteLine("     [--grouplimit]  Message group limit. Optional.");
            Console.WriteLine();
            Console.WriteLine("# To get authorization token:");
            Console.WriteLine(" - Open Discord app");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Press Ctrl+Shift+I");
            Console.WriteLine(" - Navigate to Application tab");
            Console.WriteLine(" - Expand Storage > Local Storage > https://discordapp.com");
            Console.WriteLine(" - Find \"token\" under key and copy the value");
            Console.WriteLine();
            Console.WriteLine("# To get channel ID:");
            Console.WriteLine(" - Open Discord app");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Go to any channel you want to export");
            Console.WriteLine(" - Press Ctrl+Shift+I");
            Console.WriteLine(" - Navigate to Console tab");
            Console.WriteLine(" - Type \"document.URL\" and press Enter");
            Console.WriteLine(" - Copy the long sequence of numbers after last slash");
        }

        private static CliOptions ParseOptions(string[] args)
        {
            var argsParser = new FluentCommandLineParser<CliOptions>();

            var settings = Container.SettingsService;

            argsParser.Setup(o => o.Token).As('t', "token").Required();
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

            // Export
            var vm = Container.MainViewModel;
            vm.ExportAsync(
                options.Token,
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