using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting;

internal partial class MessageExporter(ExportContext context, int initialPartitionIndex = 0)
    : IAsyncDisposable
{
    private int _partitionIndex = initialPartitionIndex;
    private MessageWriter? _writer;

    public long MessagesExported { get; private set; }

    private async ValueTask<MessageWriter> InitializeWriterAsync(
        CancellationToken cancellationToken = default
    )
    {
        // Ensure that the partition limit has not been reached
        if (
            _writer is not null
            && context.Request.PartitionLimit.IsReached(
                _writer.MessagesWritten,
                _writer.BytesWritten
            )
        )
        {
            await UninitializeWriterAsync(cancellationToken);
            _partitionIndex++;
        }

        // Writer is still valid, return
        if (_writer is not null)
            return _writer;

        Directory.CreateDirectory(context.Request.OutputDirPath);
        var filePath = GetPartitionFilePath(context.Request.OutputFilePath, _partitionIndex);

        var writer = CreateMessageWriter(filePath, context.Request.Format, context);
        await writer.WritePreambleAsync(cancellationToken);

        return _writer = writer;
    }

    private async ValueTask UninitializeWriterAsync(CancellationToken cancellationToken = default)
    {
        if (_writer is not null)
        {
            try
            {
                await _writer.WritePostambleAsync(cancellationToken);
            }
            // Writer must be disposed, even if it fails to write the postamble
            finally
            {
                await _writer.DisposeAsync();
                _writer = null;
            }
        }
    }

    public async ValueTask ExportMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        var writer = await InitializeWriterAsync(cancellationToken);
        await writer.WriteMessageAsync(message, cancellationToken);
        MessagesExported++;
    }

    public async ValueTask DisposeAsync()
    {
        // If not messages were written, force the creation of an empty file
        if (MessagesExported <= 0)
            _ = await InitializeWriterAsync();

        await UninitializeWriterAsync();
    }
}

internal partial class MessageExporter
{
    private static string GetPartitionFilePath(string baseFilePath, int partitionIndex)
    {
        // First partition, don't change the file name
        if (partitionIndex <= 0)
            return baseFilePath;

        // Inject partition index into the file name
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFilePath);
        var fileExt = Path.GetExtension(baseFilePath);
        var fileName = $"{fileNameWithoutExt} [part {partitionIndex + 1}]{fileExt}";
        var dirPath = Path.GetDirectoryName(baseFilePath);

        return !string.IsNullOrWhiteSpace(dirPath) ? Path.Combine(dirPath, fileName) : fileName;
    }

    private static MessageWriter CreateMessageWriter(
        string filePath,
        ExportFormat format,
        ExportContext context
    )
    {
        // Don't accidentally overwrite anything
        if (File.Exists(filePath))
        {
            throw new InvalidOperationException(
                "Error: An exported file already exists. This should never happen."
            );
        }

        var file = File.Create(filePath);
        return format switch
        {
            ExportFormat.PlainText => new PlainTextMessageWriter(file, context),
            ExportFormat.Csv => new CsvMessageWriter(file, context),
            ExportFormat.HtmlDark => new HtmlMessageWriter(file, context, "Dark"),
            ExportFormat.HtmlLight => new HtmlMessageWriter(file, context, "Light"),
            ExportFormat.Json => new JsonMessageWriter(file, context),
            _ => throw new ArgumentOutOfRangeException(
                nameof(format),
                $"Unknown export format '{format}'."
            ),
        };
    }

    /// <summary>
    /// Retrieves and returns the (approximate) Snowflake of the last written message in the Discord channel that has
    /// previously been exported to the given file path in the given format.
    ///
    /// If the used format is JSON, this returns its exact Snowflake (which contains the precise message timestamp and
    /// index and can therefore correctly determine which messages succeed it).
    /// Otherwise, it returns an approximate Snowflake (which only contains the (possibly approximate) message
    /// timestamp and may therefore label slightly earlier messages as succeeding).
    ///
    /// This automatically determines the number of partitions the export has been split up into and uses the last one.
    /// </summary>
    /// <param name="baseFilePath">
    /// The path of the first partition of the Discord channel export whose last message's timestamp should be returned.
    /// </param>
    /// <param name="format">
    /// The format of the Discord channel export whose last message's timestamp should be returned.
    /// </param>
    /// <returns>
    /// The (approximate) Snowflake of the last written message in the Discord channel that has been previously
    /// exported to the given file path in the given format.
    /// Null, if the Discord channel hasn't previously been exported or if the export doesn't include any message.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the last Discord channel export partition file isn't correctly formatted according to the given
    /// format.
    /// </exception>
    public static Snowflake? GetLastMessageSnowflake(string baseFilePath, ExportFormat format)
    {
        var partitionAmounts = GetPartitionCount(baseFilePath);
        var lastPartitionFile = GetPartitionFilePath(baseFilePath, partitionAmounts - 1);

        var fileInfo = new FileInfo(lastPartitionFile);
        if (fileInfo.Length == 0)
            return null;

        return format switch
        {
            ExportFormat.PlainText => PlainTextMessageWriter.GetLastMessageDate(lastPartitionFile),
            ExportFormat.Csv => CsvMessageWriter.GetLastMessageDate(lastPartitionFile),
            ExportFormat.HtmlDark or ExportFormat.HtmlLight => HtmlMessageWriter.GetLastMessageDate(
                lastPartitionFile
            ),
            ExportFormat.Json => JsonMessageWriter.GetLastMessageSnowflake(lastPartitionFile),
            _ => throw new ArgumentOutOfRangeException(
                nameof(format),
                $"Unknown export format '{format}'."
            ),
        };
    }

    /// <summary>
    /// Removes all partitions of the previously exported Discord channel with the given base file path.
    /// </summary>
    /// <param name="baseFilePath">
    /// The path of the first partition of the Discord channel export that should be removed.
    /// </param>
    public static void RemoveExistingFiles(string baseFilePath)
    {
        var currentPartition = 0;
        while (true)
        {
            var currentFilePath = GetPartitionFilePath(baseFilePath, currentPartition);
            if (File.Exists(currentFilePath))
            {
                File.Delete(currentFilePath);
            }
            else
            {
                return;
            }
            currentPartition++;
        }
    }

    /// <summary>
    /// Determines and returns the number of partitions of the previously exported Discord channel with the given base
    /// file path.
    /// </summary>
    /// <param name="baseFilePath">
    /// The path of the first partition of the Discord channel export whose number of partitions should be returned.
    /// </param>
    /// <returns>
    /// The number of partitions of the previously exported Discord channel with the given base file path.
    /// </returns>
    public static int GetPartitionCount(string baseFilePath)
    {
        // Use linear search to quickly determine the number of partitions
        var currentPartition = 1;
        while (true)
        {
            var currentFilePath = GetPartitionFilePath(baseFilePath, currentPartition - 1);
            if (File.Exists(currentFilePath))
            {
                currentPartition *= 2;
            }
            else
            {
                break;
            }
        }

        var leftBorder = currentPartition / 2;
        var rightBorder = currentPartition;
        while (rightBorder - 1 > leftBorder)
        {
            var middle = (leftBorder + rightBorder) / 2;
            var currentFilePath = GetPartitionFilePath(baseFilePath, middle - 1);
            if (File.Exists(currentFilePath))
            {
                leftBorder = middle;
            }
            else
            {
                rightBorder = middle;
            }
        }

        return leftBorder;
    }
}
