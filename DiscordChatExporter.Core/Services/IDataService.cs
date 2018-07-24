using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IDataService
    {
        Task<Guild> GetGuildAsync(AuthToken token, string guildId);

        Task<Channel> GetChannelAsync(AuthToken token, string channelId);

        Task<IReadOnlyList<Guild>> GetUserGuildsAsync(AuthToken token);

        Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(AuthToken token);

        Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(AuthToken token, string guildId);

        Task<IReadOnlyList<Role>> GetGuildRolesAsync(AuthToken token, string guildId);

        Task<IReadOnlyList<Message>> GetChannelMessagesAsync(AuthToken token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null);

        Task<Mentionables> GetMentionablesAsync(AuthToken token, string guildId,
            IEnumerable<Message> messages);
    }
}