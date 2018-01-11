using System;
using DiscordChatExporter.Core.Models;
using Fclp;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static readonly Container Container = new Container();

        public static void Main(string[] args)
        {
            // Parse args
            var argsParser = new FluentCommandLineParser();

            string token = null;
            argsParser.Setup<string>('t', "token")
                .Callback(v => token = v)
                .Required();

            string channelId = null;
            argsParser.Setup<string>('c', "channel")
                .Callback(v => channelId = v)
                .Required();

            var exportFormat = ExportFormat.HtmlDark;
            argsParser.Setup<ExportFormat>('f', "format")
                .Callback(v => exportFormat = v);

            string filePath = null;
            argsParser.Setup<string>('o', "output")
                .Callback(v => filePath = v);

            DateTime? from = null;
            argsParser.Setup<DateTime>("datefrom")
                .Callback(v => from = v);

            DateTime? to = null;
            argsParser.Setup<DateTime>("dateto")
                .Callback(v => to = v);

            argsParser.Parse(args);

            // Init container
            Container.Init();

            // Get view model
            var vm = Container.MainViewModel;

            // Export
            vm.ExportAsync(token, channelId, filePath, exportFormat, from, to).GetAwaiter().GetResult();

            // Cleanup container
            Container.Cleanup();
        }
    }
}