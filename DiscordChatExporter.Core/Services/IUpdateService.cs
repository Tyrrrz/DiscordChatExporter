using System;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Services
{
    public interface IUpdateService
    {
        Task<Version> CheckForUpdatesAsync();

        Task PrepareUpdateAsync();

        Task ApplyUpdateAsync(bool restart = true);
    }
}