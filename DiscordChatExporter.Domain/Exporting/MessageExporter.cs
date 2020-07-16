using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MessageExporter : IAsyncDisposable
    {
        private readonly ExportContext _context;

        private readonly string _outputBaseFilePath;
        private readonly UrlProcessor _urlProcessor;

        private long _messagesCount;
        private int _partitionIndex;
        private MessageWriter? _writer;

        public MessageExporter(ExportContext context)
        {
            _context = context;

            _outputBaseFilePath = context.Request.GetOutputBaseFilePath();
            _urlProcessor = new UrlProcessor($"{_outputBaseFilePath}_Files/");
        }

        private bool IsPartitionLimitReached() =>
            _messagesCount > 0 &&
            _context.Request.PartitionLimit != null &&
            _context.Request.PartitionLimit != 0 &&
            _messagesCount % _context.Request.PartitionLimit == 0;

        private async Task ResetWriterAsync()
        {
            if (_writer != null)
            {
                await _writer.WritePostambleAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
        }

        private MessageWriter CreateMessageWriter(string filePath, ExportFormat format, ExportContext context)
        {
            // Stream will be disposed by the underlying writer
            var stream = File.Create(filePath);

            return format switch
            {
                ExportFormat.PlainText => new PlainTextMessageWriter(stream, context, _urlProcessor),
                ExportFormat.Csv => new CsvMessageWriter(stream, context, _urlProcessor),
                ExportFormat.HtmlDark => new HtmlMessageWriter(stream, context, _urlProcessor, "Dark"),
                ExportFormat.HtmlLight => new HtmlMessageWriter(stream, context, _urlProcessor, "Light"),
                ExportFormat.Json => new JsonMessageWriter(stream, context, _urlProcessor),
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unknown export format '{format}'.")
            };
        }

        private async Task<MessageWriter> GetWriterAsync()
        {
            // Ensure partition limit has not been exceeded
            if (IsPartitionLimitReached())
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }

            // Writer is still valid - return
            if (_writer != null)
                return _writer;

            var filePath = GetPartitionFilePath(_outputBaseFilePath, _partitionIndex);

            var dirPath = Path.GetDirectoryName(_outputBaseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            var writer = CreateMessageWriter(filePath, _context.Request.Format, _context);
            await writer.WritePreambleAsync();

            return _writer = writer;
        }

        public async Task ExportMessageAsync(Message message)
        {
            var writer = await GetWriterAsync();
            await writer.WriteMessageAsync(message);
            _messagesCount++;
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

            // Generate new path
            var dirPath = Path.GetDirectoryName(baseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                return Path.Combine(dirPath, fileName);

            return fileName;
        }
    }
}