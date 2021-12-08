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

    public string FormatMarkdown(string? markdown, bool isJumboAllowed = true) =>
        HtmlMarkdownVisitor.Format(ExportContext, markdown ?? "", isJumboAllowed);
}