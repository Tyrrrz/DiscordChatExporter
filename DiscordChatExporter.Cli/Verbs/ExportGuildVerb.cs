using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Internal;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Helpers;
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

            // Filter and order channels
            channels = channels.Where(c => c.Type == ChannelType.GuildTextChat).OrderBy(c => c.Name).ToArray();

            // Loop through channels
            foreach (var channel in channels)
            {
                try
                {
                    // Track progress
                    Console.Write($"Exporting channel [{channel.Name}]... ");
                    using (var progress = new InlineProgress())
                    {
                        // Get chat log
                        var chatLog = await dataService.GetChatLogAsync(Options.GetToken(), channel,
                            Options.After, Options.Before, progress);

                        // Generate default file name
                        var fileName = ExportHelper.GetDefaultExportFileName(Options.ExportFormat, chatLog.Guild,
                            chatLog.Channel, Options.After, Options.Before);

                        // Generate file path
                        var filePath = Path.Combine(Options.OutputPath ?? "", fileName);

                        // Export
                        exportService.ExportChatLog(chatLog, filePath, Options.ExportFormat, Options.PartitionLimit);
                    }
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.Error.WriteLine("You don't have access to this channel");
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.Error.WriteLine("This channel doesn't exist");
                }
            }
        }
    }
}