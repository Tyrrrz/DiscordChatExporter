using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MessageExporter : IAsyncDisposable
    {
        private readonly ExportOptions _options;
        private readonly ExportContext _context;

        private long _renderedMessageCount;
        private int _partitionIndex;
        private MessageWriter? _writer;

        public MessageExporter(ExportOptions options, ExportContext context)
        {
            _options = options;
            _context = context;
        }

        private bool IsPartitionLimitReached() =>
            _renderedMessageCount > 0 &&
            _options.PartitionLimit != null &&
            _options.PartitionLimit != 0 &&
            _renderedMessageCount % _options.PartitionLimit == 0;

        private async Task ResetWriterAsync()
        {
            if (_writer != null)
            {
                await _writer.WritePostambleAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
        }

        private async Task<MessageWriter> GetWriterAsync()
        {
            // Ensure partition limit is not exceeded
            if (IsPartitionLimitReached())
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }

            // Writer is still valid - return
            if (_writer != null)
                return _writer;

            var filePath = GetPartitionFilePath(_options.BaseFilePath, _partitionIndex);

            var dirPath = Path.GetDirectoryName(_options.BaseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            var writer = CreateMessageWriter(filePath, _options.Format, _context);
            await writer.WritePreambleAsync();

            return _writer = writer;
        }

        public async Task ExportMessageAsync(Message message)
        {
            var writer = await GetWriterAsync();
            await writer.WriteMessageAsync(message);
            _renderedMessageCount++;
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

        private static MessageWriter CreateMessageWriter(string filePath, ExportFormat format, ExportContext context)
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
}