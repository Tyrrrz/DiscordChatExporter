using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Formatters;

namespace DiscordChatExporter.Core.Rendering
{
    public partial class MessageRenderer : IAsyncDisposable
    {
        private readonly RenderOptions _options;
        private readonly RenderContext _context;

        private long _renderedMessageCount;
        private int _partitionIndex;
        private MessageWriterBase? _writer;

        public MessageRenderer(RenderOptions options, RenderContext context)
        {
            _options = options;
            _context = context;
        }

        private async Task InitializeWriterAsync()
        {
            // Get partition file path
            var filePath = GetPartitionFilePath(_options.BaseFilePath, _partitionIndex);

            // Create output directory
            var dirPath = Path.GetDirectoryName(_options.BaseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            // Create writer
            _writer = CreateMessageWriter(filePath, _options.Format, _context);

            // Write preamble
            await _writer.WritePreambleAsync();
        }

        private async Task ResetWriterAsync()
        {
            if (_writer != null)
            {
                // Write postamble
                await _writer.WritePostambleAsync();

                // Flush
                await _writer.DisposeAsync();
                _writer = null;
            }
        }

        public async Task RenderMessageAsync(Message message)
        {
            // Ensure underlying writer is initialized
            if (_writer == null)
                await InitializeWriterAsync();

            // Render the actual message
            await _writer!.WriteMessageAsync(message);

            // Increment count
            _renderedMessageCount++;

            // Shift partition if necessary
            if (_options.PartitionLimit != null &&
                _options.PartitionLimit != 0 &&
                _renderedMessageCount % _options.PartitionLimit == 0)
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }
        }

        public async ValueTask DisposeAsync() => await ResetWriterAsync();
    }

    public partial class MessageRenderer
    {
        private static string GetPartitionFilePath(string baseFilePath, int partitionIndex)
        {
            // First partition - no changes
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

        private static MessageWriterBase CreateMessageWriter(string filePath, ExportFormat format, RenderContext context)
        {
            // Create a stream (it will get disposed by the writer)
            var stream = File.Create(filePath);

            // Create formatter
            if (format == ExportFormat.PlainText)
                return new PlainTextMessageWriter(stream, context);

            if (format == ExportFormat.Csv)
                return new CsvMessageWriter(stream, context);

            if (format == ExportFormat.HtmlDark)
                return new HtmlMessageWriter(stream, context, "Dark");

            if (format == ExportFormat.HtmlLight)
                return new HtmlMessageWriter(stream, context, "Light");

            if (format == ExportFormat.Json)
                return new JsonMessageWriter(stream, context);

            throw new InvalidOperationException($"Unknown export format [{format}].");
        }
    }
}