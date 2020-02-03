using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;

namespace DiscordChatExporter.Core.Rendering.Formatters
{
    public class PlainTextMessageWriter : MessageWriterBase
    {
        private readonly TextWriter _writer;

        private long _messageCount;

        public PlainTextMessageWriter(Stream stream, RenderContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        public override async Task WritePreambleAsync()
        {
            await _writer.WriteLineAsync(PlainTextRenderingLogic.FormatPreamble(Context));
        }

        public override async Task WriteMessageAsync(Message message)
        {
            await _writer.WriteLineAsync(PlainTextRenderingLogic.FormatMessage(Context, message));
            await _writer.WriteLineAsync();

            _messageCount++;
        }

        public override async Task WritePostambleAsync()
        {
            await _writer.WriteLineAsync();
            await _writer.WriteLineAsync(PlainTextRenderingLogic.FormatPostamble(_messageCount));
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}