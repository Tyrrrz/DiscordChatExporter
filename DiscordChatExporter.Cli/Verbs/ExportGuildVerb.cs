using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class ExportGuildVerb : Verb<ExportGuildOptions>
    {
        public ExportGuildVerb(ExportGuildOptions options)
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

            // Get channels
            var channels = await dataService.GetGuildChannelsAsync(Options.GetToken(), Options.GuildId);

            // Loop through channels
            foreach (var channel in channels)
            {
                bool channelAccess = await dataService.GetApiResponseAsync(Options.GetToken(), "channels", channel.Id);
                if (channelAccess)
                {
                    // Get chat log
                    var chatLog = await dataService.GetChatLogAsync(Options.GetToken(), channel.Id,
                        Options.After, Options.Before);
                    // Generate file path if not set
                    var filePath = Options.FilePath;
                    if (filePath == null || filePath.EndsWith("/") || filePath.EndsWith("\\"))
                    {
                        filePath += $"{chatLog.Guild.Name} - {chatLog.Channel.Name}.{Options.ExportFormat.GetFileExtension()}"
                            .Replace(Path.GetInvalidFileNameChars(), '_');
                    }

                    // Export
                    exportService.ExportChatLog(chatLog, filePath, Options.ExportFormat, Options.PartitionLimit);

                    // Print result
                    Console.WriteLine($"Exported chat to [{filePath}]");
                }
            }
            
        }
    }
}