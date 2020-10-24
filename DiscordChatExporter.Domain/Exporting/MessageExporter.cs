using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MessageExporter : IAsyncDisposable
    {
        private readonly ExportContext _context;

        private long _messageCount;
        private int _partitionIndex;
        private IEnumerable<MessageWriter>? _writers;

        public MessageExporter(ExportContext context)
        {
            _context = context;
        }

        private bool IsPartitionLimitReached() =>
            _messageCount > 0 &&
            _context.Request.PartitionLimit != null &&
            _context.Request.PartitionLimit != 0 &&
            _messageCount % _context.Request.PartitionLimit == 0;

        private async ValueTask ResetWriterAsync()
        {
            if (_writers != null)
            {
                await _writers.ParallelForEachAsync(async w =>
                {
                    await w.WritePostambleAsync();
                    await w.DisposeAsync();
                });

                _writers = null;
            }
        }

        private async ValueTask<IEnumerable<MessageWriter>> GetWritersAsync()
        {
            // Ensure partition limit has not been exceeded
            if (IsPartitionLimitReached())
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }

            // Writer is still valid - return
            if (_writers != null)
                return _writers;

            var request = _context.Request;

            var writers = request.Formats.Select(format =>
            {
                var outputFilePath = request.GetOutputFilePathForFormat(format);
                var filePath = GetPartitionFilePath(outputFilePath, _partitionIndex);

                var dirPath = request.OutputBaseDirPath;
                if (!string.IsNullOrWhiteSpace(dirPath))
                    Directory.CreateDirectory(dirPath);

                var writer = CreateMessageWriter(filePath, format, _context);
                return writer;
            }).ToList();

            await writers.ParallelForEachAsync(async w => await w.WritePreambleAsync());

            return _writers = writers;
        }

        public async ValueTask ExportMessageAsync(Message message)
        {
            var writers = await GetWritersAsync();
            await writers.ParallelForEachAsync(async w => await w.WriteMessageAsync(message));
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
    }
}