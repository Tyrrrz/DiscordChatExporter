using System;
using DiscordChatExporter.Models;
using Fclp;

namespace DiscordChatExporter
{
    public class Cli
    {
        private readonly Container _container;

        public Cli()
        {
            _container = new Container();
        }

        public void Run(string[] args)
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
            _container.Init();

            // Get view model
            var vm = _container.CliViewModel;

            // Export
            vm.ExportAsync(token, channelId, filePath, exportFormat, from, to).GetAwaiter().GetResult();

            // Cleanup container
            _container.Cleanup();
        }
    }
}