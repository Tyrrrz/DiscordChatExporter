using System;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;
using DiscordChatExporter.Cli.Internal;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Helpers;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Commands
{
    public abstract class ExportCommandBase : TokenCommandBase
    {
        protected SettingsService SettingsService { get; }

        protected ExportService ExportService { get; }


        [CommandOption("format", 'f', Description = "Output file format.")]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.HtmlDark;

        [CommandOption("output", 'o', Description = "Output file or directory path.")]
        public string OutputPath { get; set; }

        [CommandOption("after",Description = "Limit to messages sent after this date.")]
        public DateTimeOffset? After { get; set; }

        [CommandOption("before",Description = "Limit to messages sent before this date.")]
        public DateTimeOffset? Before { get; set; }

        [CommandOption("partition", 'p', Description = "Split output into partitions limited to this number of messages.")]
        public int? PartitionLimit { get; set; }

        [CommandOption("dateformat", Description = "Date format used in output.")]
        public string DateFormat { get; set; }

        protected ExportCommandBase(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(dataService)
        {
            SettingsService = settingsService;
            ExportService = exportService;
        }

        protected async Task ExportChannelAsync(IConsole console, Channel channel)
        {
            // Configure settings
            if (!DateFormat.IsNullOrWhiteSpace())
                SettingsService.DateFormat = DateFormat;

            console.Output.Write($"Exporting channel [{channel.Name}]... ");
            using (var progress = new InlineProgress(console))
            {
                // Get chat log
                var chatLog = await DataService.GetChatLogAsync(GetToken(), channel, After, Before, progress);

                // Generate file path if not set or is a directory
                var filePath = OutputPath;
                if (filePath.IsNullOrWhiteSpace() || ExportHelper.IsDirectoryPath(filePath))
                {
                    // Generate default file name
                    var fileName = ExportHelper.GetDefaultExportFileName(ExportFormat, chatLog.Guild,
                        chatLog.Channel, After, Before);

                    // Combine paths
                    filePath = Path.Combine(filePath ?? "", fileName);
                }

                // Export
                await ExportService.ExportChatLogAsync(chatLog, filePath, ExportFormat, PartitionLimit);

                // Report successful completion
                progress.ReportCompletion();
            }
        }
    }
}