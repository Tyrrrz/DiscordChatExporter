using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public abstract class MessageRendererBase : IMessageRenderer
    {
        protected TextWriter Writer { get; }

        protected RenderContext Context { get; }

        protected MessageRendererBase(TextWriter writer, RenderContext context)
        {
            Writer = writer;
            Context = context;
        }

        public abstract Task RenderMessageAsync(Message message);

        public virtual ValueTask DisposeAsync() => default;
    }
}