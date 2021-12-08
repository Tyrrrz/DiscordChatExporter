namespace DiscordChatExporter.Core.Exporting.Writers.Html;

internal class PostambleTemplateContext
{
    public ExportContext ExportContext { get; }

    public long MessagesWritten { get; }

    public PostambleTemplateContext(ExportContext exportContext, long messagesWritten)
    {
        ExportContext = exportContext;
        MessagesWritten = messagesWritten;
    }
}