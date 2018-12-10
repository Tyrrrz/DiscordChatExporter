using System;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Helpers;
using DiscordChatExporter.Core.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class ExportChatVerb : Verb<ExportChatOptions>
    {
        public ExportChatVerb(ExportChatOptions options)
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

            // Get chat log
            var chatLog = await dataService.GetChatLogAsync(Options.GetToken(), Options.ChannelId, 
                Options.After, Options.Before);

            // Generate file path if not set or is a directory
            var filePath = Options.FilePath;
            if (filePath == null || filePath.EndsWith("/") || filePath.EndsWith("\\"))
            {
                // Generate default file name
                var defaultFileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, chatLog.Guild,
                    chatLog.Channel, Options.After, Options.Before);

                // Append the file name to the file path
                filePath += defaultFileName;
            }

            // Export
            exportService.ExportChatLog(chatLog, filePath, Options.ExportFormat, Options.PartitionLimit);

            // Print result
            Console.WriteLine($"Exported chat to [{filePath}]");
        }
    }
}