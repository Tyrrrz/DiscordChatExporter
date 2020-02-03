using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;

namespace DiscordChatExporter.Core.Rendering.Formatters
{
    public class CsvMessageWriter : MessageWriterBase
    {
        private readonly TextWriter _writer;

        public CsvMessageWriter(Stream stream, RenderContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        public override async Task WritePreambleAsync()
        {
            await _writer.WriteLineAsync(CsvRenderingLogic.FormatHeader(Context));
        }

        public override async Task WriteMessageAsync(Message message)
        {
            await _writer.WriteLineAsync(CsvRenderingLogic.FormatMessage(Context, message));
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}