using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.ViewModels
{
    public class MainViewModel : IMainViewModel
    {
        private readonly IDataService _dataService;
        private readonly IMessageGroupService _messageGroupService;
        private readonly IExportService _exportService;

        public MainViewModel(IDataService dataService, IMessageGroupService messageGroupService,
            IExportService exportService)
        {
            _dataService = dataService;
            _messageGroupService = messageGroupService;
            _exportService = exportService;
        }

        public async Task ExportAsync(string token, string channelId, string filePath, ExportFormat format,
            DateTime? from, DateTime? to)
        {
            // Get channel and guild
            var channel = await _dataService.GetChannelAsync(token, channelId);
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : await _dataService.GetGuildAsync(token, channel.GuildId);

            // Generate file path if not set
            if (filePath.IsBlank())
            {
                filePath = $"{guild.Name} - {channel.Name}.{format.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
            }

            // Create progress handler
            // TODO
            var progressHandler = new Progress<double>(p => { });

            // Get messages
            var messages = await _dataService.GetChannelMessagesAsync(token, channel.Id, from, to, progressHandler);

            // Group messages
            var messageGroups = _messageGroupService.GroupMessages(messages);

            // Get mentionables
            var mentionables = await _dataService.GetMentionablesAsync(token, guild.Id, messages);

            // Create log
            var log = new ChatLog(guild, channel, messageGroups, mentionables);

            // Export
            _exportService.Export(format, filePath, log);
        }
    }
}