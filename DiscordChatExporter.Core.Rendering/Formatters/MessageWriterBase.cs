using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering.Formatters
{
    public abstract class MessageWriterBase : IAsyncDisposable
    {
        protected TextWriter Writer { get; }

        protected RenderContext Context { get; }

        protected MessageWriterBase(TextWriter writer, RenderContext context)
        {
            Writer = writer;
            Context = context;
        }

        public virtual Task WritePreambleAsync() => Task.CompletedTask;

        public abstract Task WriteMessageAsync(Message message);

        public virtual Task WritePostambleAsync() => Task.CompletedTask;

        public async ValueTask DisposeAsync() => await Writer.DisposeAsync();
    }
}