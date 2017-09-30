using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IDataService
    {
        Task<IReadOnlyList<Guild>> GetGuildsAsync(string token);

        Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(string token);

        Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string token, string guildId);

        Task<IReadOnlyList<Message>> GetChannelMessagesAsync(string token, string channelId,
            DateTime? from, DateTime? to);
    }
}