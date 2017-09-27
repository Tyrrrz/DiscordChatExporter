using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Guild>> GetGuildsAsync(string token);

        Task<IEnumerable<Channel>> GetDirectMessageChannelsAsync(string token);

        Task<IEnumerable<Channel>> GetGuildChannelsAsync(string token, string guildId);

        Task<IEnumerable<Message>> GetChannelMessagesAsync(string token, string channelId);
    }
}