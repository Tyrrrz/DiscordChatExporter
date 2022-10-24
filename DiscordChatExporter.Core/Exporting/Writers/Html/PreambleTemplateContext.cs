using System.Threading.Tasks;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;

namespace DiscordChatExporter.Core.Exporting.Writers.Html;

internal class PreambleTemplateContext
{
    public ExportContext ExportContext { get; }

    public string ThemeName { get; }

    public PreambleTemplateContext(ExportContext exportContext, string themeName)
    {
        ExportContext = exportContext;
        ThemeName = themeName;
    }

    public ValueTask<string> FormatMarkdownAsync(string? markdown, bool isJumboAllowed = true) =>
        HtmlMarkdownVisitor.FormatAsync(ExportContext, markdown ?? "", isJumboAllowed);
}