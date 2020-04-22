using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal abstract class MessageWriter : IAsyncDisposable
    {
        protected Stream Stream { get; }

        protected ExportContext Context { get; }

        protected MessageWriter(Stream stream, ExportContext context)
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