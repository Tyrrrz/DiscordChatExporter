using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;

namespace DiscordChatExporter.Core.Rendering
{
    public class CsvMessageRenderer : MessageRendererBase
    {
        private bool _isHeaderRendered;

        public CsvMessageRenderer(string filePath, RenderContext context)
            : base(filePath, context)
        {
        }

        public override async Task RenderMessageAsync(Message message)
        {
            // Render header if it's the first entry
            if (!_isHeaderRendered)
            {
                await Writer.WriteLineAsync(CsvRenderingLogic.FormatHeader(Context));
                _isHeaderRendered = true;
            }

            await Writer.WriteLineAsync(CsvRenderingLogic.FormatMessage(Context, message));
        }
    }
}
