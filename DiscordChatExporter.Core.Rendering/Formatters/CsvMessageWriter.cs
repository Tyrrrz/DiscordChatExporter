using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;

namespace DiscordChatExporter.Core.Rendering.Formatters
{
    public class CsvMessageWriter : MessageWriterBase
    {
        public CsvMessageWriter(TextWriter writer, RenderContext context)
            : base(writer, context)
        {
        }

        public override async Task WritePreambleAsync()
        {
            await Writer.WriteLineAsync(CsvRenderingLogic.FormatHeader(Context));
        }

        public override async Task WriteMessageAsync(Message message)
        {
            await Writer.WriteLineAsync(CsvRenderingLogic.FormatMessage(Context, message));
        }
    }
}