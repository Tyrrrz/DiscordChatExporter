using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal abstract class MessageWriter : IAsyncDisposable
    {
        private readonly UrlProcessor _urlProcessor;

        protected Stream Stream { get; }

        protected ExportContext Context { get; }

        protected MessageWriter(Stream stream, ExportContext context, UrlProcessor urlProcessor)
        {
            Stream = stream;
            Context = context;
            _urlProcessor = urlProcessor;
        }

        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        protected async Task<string?> ResolveUrlAsync(string? url) =>
            !string.IsNullOrWhiteSpace(url) && Context.Request.RewriteMedia
                ? await _urlProcessor.ConvertAsync(url).ConfigureAwait(false)
                : url;

        public virtual Task WritePreambleAsync() => Task.CompletedTask;

        public abstract Task WriteMessageAsync(Message message);

        public virtual Task WritePostambleAsync() => Task.CompletedTask;

        public virtual async ValueTask DisposeAsync() => await Stream.DisposeAsync();
    }
}