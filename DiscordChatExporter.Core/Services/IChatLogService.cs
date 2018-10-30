using System;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IChatLogService
    {
        Task<ChatLog> GetChatLogAsync(AuthToken token, Guild guild, Channel channel, 
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null);

        Task<ChatLog> GetChatLogAsync(AuthToken token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null);
    }
}