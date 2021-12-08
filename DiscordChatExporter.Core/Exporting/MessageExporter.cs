using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Writers;

namespace DiscordChatExporter.Core.Exporting;

internal partial class MessageExporter : IAsyncDisposable
{
    private readonly ExportContext _context;

    private int _partitionIndex;
    private MessageWriter? _writer;

    public MessageExporter(ExportContext context)
    {
        _context = context;
    }

    private async ValueTask ResetWriterAsync(CancellationToken cancellationToken = default)
    {
        if (_writer is not null)
        {
            await _writer.WritePostambleAsync(cancellationToken);
            await _writer.DisposeAsync();
            _writer = null;
        }
    }

    private async ValueTask<MessageWriter> GetWriterAsync(CancellationToken cancellationToken = default)
    {
        // Ensure partition limit has not been reached
        if (_writer is not null &&
            _context.Request.PartitionLimit.IsReached(_writer.MessagesWritten, _writer.BytesWritten))
        {
            await ResetWriterAsync(cancellationToken);
            _partitionIndex++;
        }

        // Writer is still valid - return
        if (_writer is not null)
            return _writer;

        var filePath = GetPartitionFilePath(_context.Request.OutputBaseFilePath, _partitionIndex);

        var dirPath = Path.GetDirectoryName(_context.Request.OutputBaseFilePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        var writer = CreateMessageWriter(filePath, _context.Request.Format, _context);
        await writer.WritePreambleAsync(cancellationToken);

        return _writer = writer;
    }

    public async ValueTask ExportMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        var writer = await GetWriterAsync(cancellationToken);
        await writer.WriteMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync() => await ResetWriterAsync();
}

internal partial class MessageExporter
{
    private static string GetPartitionFilePath(string baseFilePath, int partitionIndex)
    {
        // First partition - don't change file name
        if (partitionIndex <= 0)
            return baseFilePath;

        // Inject partition index into file name
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFilePath);
        var fileExt = Path.GetExtension(baseFilePath);
        var fileName = $"{fileNameWithoutExt} [part {partitionIndex + 1}]{fileExt}";
        var dirPath = Path.GetDirectoryName(baseFilePath);

        return !string.IsNullOrWhiteSpace(dirPath)
            ? Path.Combine(dirPath, fileName)
            : fileName;
    }

    private static MessageWriter CreateMessageWriter(
        string filePath,
        ExportFormat format,
        ExportContext context)
    {
        // Stream will be disposed by the underlying writer
        var stream = File.Create(filePath);

        return format switch
        {
            ExportFormat.PlainText => new PlainTextMessageWriter(stream, context),
            ExportFormat.Csv => new CsvMessageWriter(stream, context),
            ExportFormat.HtmlDark => new HtmlMessageWriter(stream, context, "Dark"),
            ExportFormat.HtmlLight => new HtmlMessageWriter(stream, context, "Light"),
            ExportFormat.Json => new JsonMessageWriter(stream, context),
            _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unknown export format '{format}'.")
        };
    }
}