using System.Linq;
using Markdig.Syntax.Inlines;

namespace DiscordChatExporter.Gui.Utils.Extensions;

internal static class MarkdigExtensions
{
    extension(Inline inline)
    {
        public string GetInnerText() =>
            inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                ContainerInline container => string.Concat(container.Select(c => c.GetInnerText())),
                _ => string.Empty,
            };
    }
}
