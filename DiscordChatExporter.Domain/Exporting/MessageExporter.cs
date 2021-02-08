using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ByteSizeLib;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MessageExporter : IAsyncDisposable
    {

        private readonly ExportContext _context;

        private long _messageCount;
        private int _partitionIndex;
        private MessageWriter? _writer;
        private IPartitioner _partitioner;

        public MessageExporter(ExportContext context)
        {
            _context = context;
            _partitioner = CreatePartitioner(context.Request.PartitionLimit);
        }

        private bool IsPartitionLimitReached()
        {
            if (_writer == null)
            {
                throw new Exception("Cannot check for partition limit being reached when writer is null.");
            }
            return _partitioner.IsLimitReached(_messageCount, _writer.SizeInBytes);
        }

        private async ValueTask ResetWriterAsync()
        {
            if (_writer != null)
            {
                await _writer.WritePostambleAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
        }

        private async ValueTask<MessageWriter> GetWriterAsync()
        {
            // Ensure partition limit has not been exceeded
            if (_writer != null && IsPartitionLimitReached())
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }

            // Writer is still valid - return
            if (_writer != null)
                return _writer;

            var filePath = GetPartitionFilePath(_context.Request.OutputBaseFilePath, _partitionIndex);

            var dirPath = Path.GetDirectoryName(_context.Request.OutputBaseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            var writer = CreateMessageWriter(filePath, _context.Request.Format, _context);
            await writer.WritePreambleAsync();

            return _writer = writer;
        }

        public async ValueTask ExportMessageAsync(Message message)
        {
            var writer = await GetWriterAsync();
            await writer.WriteMessageAsync(message);
            _messageCount++;
        }

        public async ValueTask DisposeAsync() => await ResetWriterAsync();
    }

    internal partial class MessageExporter
    {
        private static string GetPartitionFilePath(
            string baseFilePath,
            int partitionIndex)
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

        private static IPartitioner CreatePartitioner(string? requestedPartitionLimit)
        {
            if (requestedPartitionLimit == null) return new NullPartitioner();
            
            if (ByteSize.TryParse(requestedPartitionLimit, out ByteSize filesize))
            {
                return new FileSizePartitioner((long) filesize.Bytes);
            }
            else
            {
                int messageLimit = int.Parse(requestedPartitionLimit);
                return new MessageCountPartitioner(messageLimit);
            }
        }
    }
}