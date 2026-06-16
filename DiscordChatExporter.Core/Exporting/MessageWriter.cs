using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting;

internal abstract class MessageWriter(Stream stream, ExportContext context) : IAsyncDisposable
{
    protected Stream Stream { get; } = stream;

    protected ExportContext Context { get; } = context;

    public long MessagesWritten { get; private set; }

    public long BytesWritten => Stream.Length;

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
