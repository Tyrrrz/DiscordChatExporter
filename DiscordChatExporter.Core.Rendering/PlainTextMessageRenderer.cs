using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;

namespace DiscordChatExporter.Core.Rendering
{
    public class PlainTextMessageRenderer : MessageRendererBase
    {
        private bool _isPreambleRendered;

        public PlainTextMessageRenderer(string filePath, RenderContext context)
            : base(filePath, context)
        {
        }

        public override async Task RenderMessageAsync(Message message)
        {
            // Render preamble if it's the first entry
            if (!_isPreambleRendered)
            {
                await Writer.WriteLineAsync(PlainTextRenderingLogic.FormatPreamble(Context));
                _isPreambleRendered = true;
            }

            await Writer.WriteLineAsync(PlainTextRenderingLogic.FormatMessage(Context, message));
            await Writer.WriteLineAsync();
        }
    }
}