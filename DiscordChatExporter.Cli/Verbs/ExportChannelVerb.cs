using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Internal;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Helpers;
using DiscordChatExporter.Core.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class ExportChannelVerb : Verb<ExportChannelOptions>
    {
        public ExportChannelVerb(ExportChannelOptions options)
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get services
            var settingsService = Container.Instance.Get<SettingsService>();
            var dataService = Container.Instance.Get<DataService>();
            var exportService = Container.Instance.Get<ExportService>();

            // Configure settings
            if (Options.DateFormat.IsNotBlank())
                settingsService.DateFormat = Options.DateFormat;
            if (Options.MessageGroupLimit > 0)
                settingsService.MessageGroupLimit = Options.MessageGroupLimit;

            // Track progress
            Console.Write($"Exporting channel [{Options.ChannelId}]... ");
            using (var progress = new InlineProgress())
            {
                // Get chat log
                var chatLog = await dataService.GetChatLogAsync(Options.GetToken(), Options.ChannelId,
                    Options.After, Options.Before, progress);

                // Generate file path if not set or is a directory
                var filePath = Options.OutputPath;
                if (filePath.IsBlank() || ExportHelper.IsDirectoryPath(filePath))
                {
                    // Generate default file name
                    var fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, chatLog.Guild,
                        chatLog.Channel, Options.After, Options.Before);

                    // Combine paths
                    filePath = Path.Combine(filePath ?? "", fileName);
                }

                // Export
                exportService.ExportChatLog(chatLog, filePath, Options.ExportFormat, Options.PartitionLimit);
            }
        }
    }
}