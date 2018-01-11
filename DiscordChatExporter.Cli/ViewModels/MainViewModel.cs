using System;
using System.IO;
using System.Linq;
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

        public async Task ExportAsync(string token, string channelId, string filePath, ExportFormat format, DateTime? from,
            DateTime? to)
        {
            // Find guild and channel by channel ID
            Guild selectedGuild;
            Channel selectedChannel;

            // Find among DM channels
            {
                var channels = await _dataService.GetDirectMessageChannelsAsync(token);
                selectedChannel = channels.FirstOrDefault(c => c.Id == channelId);
                selectedGuild = Guild.DirectMessages;
            }

            // Find among guild channels
            if (selectedChannel == null)
            {
                var guilds = await _dataService.GetUserGuildsAsync(token);
                foreach (var guild in guilds)
                {
                    var channels = await _dataService.GetGuildChannelsAsync(token, guild.Id);
                    selectedChannel = channels.FirstOrDefault(c => c.Id == channelId);
                    selectedGuild = Guild.DirectMessages;

                    if (selectedChannel != null)
                        break;
                }
            }

            // Generate file path if not set
            if (filePath.IsBlank())
            {
                filePath = $"{selectedGuild} - {selectedChannel}.{format.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
            }

            // Get messages
            var messages = await _dataService.GetChannelMessagesAsync(token, channelId, from, to);

            // Group them
            var messageGroups = _messageGroupService.GroupMessages(messages);

            // Create log
            var log = new ChannelChatLog(selectedGuild, selectedChannel, messageGroups, messages.Count);

            // Export
            await _exportService.ExportAsync(format, filePath, log);
        }
    }
}