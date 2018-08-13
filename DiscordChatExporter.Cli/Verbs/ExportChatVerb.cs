using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
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
            var container = new Container();
            var settingsService = container.Resolve<ISettingsService>();
            var dataService = container.Resolve<IDataService>();
            var messageGroupService = container.Resolve<IMessageGroupService>();
            var exportService = container.Resolve<IExportService>();

            // Configure settings
            if (Options.DateFormat.IsNotBlank())
                settingsService.DateFormat = Options.DateFormat;
            if (Options.MessageGroupLimit > 0)
                settingsService.MessageGroupLimit = Options.MessageGroupLimit;

            // Get channel and guild
            var channel = await dataService.GetChannelAsync(Options.GetToken(), Options.ChannelId);
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : await dataService.GetGuildAsync(Options.GetToken(), channel.GuildId);

            // Generate file path if not set
            var filePath = Options.FilePath;
            if (filePath == null || filePath.EndsWith("/") || filePath.EndsWith("\\"))
            {
                filePath += $"{guild.Name} - {channel.Name}.{Options.ExportFormat.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
            }

            // TODO: extract this to make it reusable across implementations
            // Get messages
            var messages =
                await dataService.GetChannelMessagesAsync(Options.GetToken(), channel.Id,
                    Options.After, Options.Before);

            // Group messages
            var messageGroups = messageGroupService.GroupMessages(messages);

            // Get mentionables
            var mentionables = await dataService.GetMentionablesAsync(Options.GetToken(), guild.Id, messages);

            // Create log
            var log = new ChatLog(guild, channel, Options.After, Options.Before, messageGroups, mentionables);

            // Export
            exportService.Export(Options.ExportFormat, filePath, log);

            // Print result
            Console.WriteLine($"Exported chat to [{filePath}]");
        }
    }
}