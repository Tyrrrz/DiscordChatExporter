using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting;

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

    public virtual ValueTask WritePreambleAsync(CancellationToken cancellationToken = default) =>
        default;

    public virtual ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        MessagesWritten++;
        return default;
    }

    public virtual ValueTask WritePostambleAsync(CancellationToken cancellationToken = default) =>
        default;

    public virtual async ValueTask DisposeAsync() => await Stream.DisposeAsync();
}
