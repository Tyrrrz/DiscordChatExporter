using System;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Services
{
    public interface IUpdateService
    {
        bool NeedRestart { get; set; }

        Task<Version> CheckPrepareUpdateAsync();

        Task FinalizeUpdateAsync();
    }
}