using System;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public class ChatLogService : IChatLogService
    {
        private readonly IDataService _dataService;
        private readonly IMessageGroupService _messageGroupService;

        public ChatLogService(IDataService dataService, IMessageGroupService messageGroupService)
        {
            _dataService = dataService;
            _messageGroupService = messageGroupService;
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, Guild guild, Channel channel,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            // Get messages
            var messages = await _dataService.GetChannelMessagesAsync(token, channel.Id, from, to, progress);

            // Group messages
            var messageGroups = _messageGroupService.GroupMessages(messages);

            // Get total message count
            var totalMessageCount = messages.Count;

            // Get mentionables
            var mentionables = await _dataService.GetMentionablesAsync(token, guild.Id, messages);

            return new ChatLog(guild, channel, from, to, messageGroups, totalMessageCount, mentionables);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            // Get channel
            var channel = await _dataService.GetChannelAsync(token, channelId);

            // Get guild
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : await _dataService.GetGuildAsync(token, channel.GuildId);

            // Get the chat log
            return await GetChatLogAsync(token, guild, channel, from, to, progress);
        }
    }
}