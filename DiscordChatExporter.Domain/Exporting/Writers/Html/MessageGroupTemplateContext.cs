using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;

namespace DiscordChatExporter.Domain.Exporting.Writers.Html
{
    internal class MessageGroupTemplateContext
    {
        public ExportContext ExportContext { get; }

        public MessageGroup MessageGroup { get; }

        public MessageGroupTemplateContext(ExportContext exportContext, MessageGroup messageGroup)
        {
            ExportContext = exportContext;
            MessageGroup = messageGroup;
        }

        public string FormatMarkdown(string? markdown, bool isJumboAllowed = true) =>
            HtmlMarkdownVisitor.Format(ExportContext, markdown ?? "", isJumboAllowed);
    }
}