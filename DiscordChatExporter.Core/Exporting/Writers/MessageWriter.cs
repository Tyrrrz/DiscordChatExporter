using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Writers
{
    internal abstract class MessageWriter : IAsyncDisposable
    {
        protected Stream Stream { get; }

        protected ExportContext Context { get; }

        public long MessagesWritten { get; private set; }

        public long BytesWritten => Stream.Length;

        protected MessageWriter(Stream stream, ExportContext context)
        {
            Stream = stream;
            Context = context;
        }

        public virtual ValueTask WritePreambleAsync() => default;

        public virtual ValueTask WriteMessageAsync(Message message)
        {
            MessagesWritten++;
            return default;
        }

        public virtual ValueTask WritePostambleAsync() => default;

        public virtual async ValueTask DisposeAsync() => await Stream.DisposeAsync();
    }
}