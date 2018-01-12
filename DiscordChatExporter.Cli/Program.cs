using System;
using DiscordChatExporter.Core.Models;
using Fclp;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static readonly Container Container = new Container();

        private static CliOptions ParseOptions(string[] args)
        {
            var argsParser = new FluentCommandLineParser<CliOptions>();

            argsParser.Setup(o => o.Token)
                .As('t', "token")
                .Required()
                .WithDescription("Discord authorization token.");

            argsParser.Setup(o => o.ChannelId)
                .As('c', "channel")
                .Required()
                .WithDescription("Discord channel ID.");

            argsParser.Setup(o => o.ExportFormat)
                .As('f', "format")
                .SetDefault(ExportFormat.HtmlDark)
                .WithDescription("Export format (PlainText/HtmlDark/HtmlLight). Optional.");

            argsParser.Setup(o => o.FilePath)
                .As('o', "output")
                .SetDefault(null)
                .WithDescription("Output file path. Optional.");

            argsParser.Setup(o => o.From)
                .As("datefrom")
                .SetDefault(null)
                .WithDescription("Messages after this date. Optional.");

            argsParser.Setup(o => o.To)
                .As("dateto")
                .SetDefault(null)
                .WithDescription("Messages before this date. Optional.");

            var parsed = argsParser.Parse(args);

            // Show help if no arguments
            if (parsed.EmptyArgs)
            {
                Console.WriteLine("=== Discord Chat Exporter (Command Line Interface) ===");
                Console.WriteLine();
                Console.WriteLine("[-t] [--token]     Discord authorization token.");
                Console.WriteLine("[-c] [--channel]   Discord channel ID.");
                Console.WriteLine("[-f] [--format]    Export format (PlainText/HtmlDark/HtmlLight). Optional.");
                Console.WriteLine("[-o] [--output]    Output file path. Optional.");
                Console.WriteLine("     [--datefrom]  Limit to messages after this date. Optional.");
                Console.WriteLine("     [--dateto]    Limit to messages before this date. Optional.");
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
            // Parse options
            var options = ParseOptions(args);

            // Init container
            Container.Init();

            // Get view model
            var vm = Container.MainViewModel;

            // Export
            vm.ExportAsync(
                options.Token,
                options.ChannelId,
                options.FilePath,
                options.ExportFormat,
                options.From,
                options.To).GetAwaiter().GetResult();

            // Cleanup container
            Container.Cleanup();
        }
    }
}