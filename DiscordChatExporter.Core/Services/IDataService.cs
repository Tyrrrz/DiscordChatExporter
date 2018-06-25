using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IDataService
    {
        Task<Guild> GetGuildAsync(string token, string guildId);

        Task<Channel> GetChannelAsync(string token, string channelId);

        Task<IReadOnlyList<Guild>> GetUserGuildsAsync(string token);

        Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(string token);

        Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string token, string guildId);

        Task<IReadOnlyList<Role>> GetGuildRolesAsync(string token, string guildId);

        Task<IReadOnlyList<Message>> GetChannelMessagesAsync(string token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null);

        Task<Mentionables> GetMentionablesAsync(string token, string guildId,
            IEnumerable<Message> messages);
    }
}