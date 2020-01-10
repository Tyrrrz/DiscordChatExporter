using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public partial class FacadeMessageRenderer : IMessageRenderer
    {
        private readonly string _baseFilePath;
        private readonly ExportFormat _format;
        private readonly RenderContext _context;

        private int _partitionIndex;
        private TextWriter _writer;
        private IMessageRenderer _innerRenderer;

        public FacadeMessageRenderer(string baseFilePath, ExportFormat format, RenderContext context)
        {
            _baseFilePath = baseFilePath;
            _format = format;
            _context = context;
        }

        private void EnsureInnerRendererInitialized()
        {
            if (_writer != null && _innerRenderer != null)
                return;

            // Get partition file path
            var filePath = GetPartitionFilePath(_baseFilePath, _partitionIndex);

            // Create output directory
            var dirPath = Path.GetDirectoryName(_baseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            // Create writer (will be disposed by renderer)
            _writer = File.CreateText(filePath);

            // Create inner renderer
            if (_format == ExportFormat.PlainText)
            {
                _innerRenderer = new PlainTextMessageRenderer(_writer, _context);
            }
            else if (_format == ExportFormat.Csv)
            {
                _innerRenderer = new CsvMessageRenderer(_writer, _context);
            }
            else if (_format == ExportFormat.HtmlDark)
            {
                _innerRenderer = new HtmlMessageRenderer(_writer, _context, "Dark");
            }
            else if (_format == ExportFormat.HtmlLight)
            {
                _innerRenderer = new HtmlMessageRenderer(_writer, _context, "Light");
            }
            else
            {
                throw new InvalidOperationException($"Unknown export format [{_format}].");
            }
        }

        public async Task NextPartitionAsync()
        {
            // Dispose writer and inner renderer
            await DisposeAsync();
            _writer = null;
            _innerRenderer = null;

            // Increment partition index
            _partitionIndex++;
        }

        public async Task RenderMessageAsync(Message message)
        {
            EnsureInnerRendererInitialized();
            await _innerRenderer.RenderMessageAsync(message);
        }

        public async ValueTask DisposeAsync()
        {
            if (_innerRenderer != null)
                await _innerRenderer.DisposeAsync();

            if (_writer != null)
                await _writer.DisposeAsync();
        }
    }

    public partial class FacadeMessageRenderer
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
    }
}