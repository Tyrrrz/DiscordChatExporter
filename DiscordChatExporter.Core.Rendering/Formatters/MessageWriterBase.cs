using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering.Formatters
{
    public abstract class MessageWriterBase : IAsyncDisposable
    {
        protected Stream Stream { get; }

        protected RenderContext Context { get; }

        protected MessageWriterBase(Stream stream, RenderContext context)
        {
            Stream = stream;
            Context = context;
        }

        public virtual Task WritePreambleAsync() => Task.CompletedTask;

        public abstract Task WriteMessageAsync(Message message);

        public virtual Task WritePostambleAsync() => Task.CompletedTask;

        public virtual async ValueTask DisposeAsync() => await Stream.DisposeAsync();
    }
}