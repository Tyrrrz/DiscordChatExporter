using System;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public interface IMessageRenderer : IAsyncDisposable
    {
        Task RenderMessageAsync(Message message);
    }
}