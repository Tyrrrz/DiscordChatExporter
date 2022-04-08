using System.Collections.Generic;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;

namespace DiscordChatExporter.Core.Exporting.Writers.Html;

internal class MessageGroupTemplateContext
{
    public ExportContext ExportContext { get; }

    public IReadOnlyList<Message> Messages { get; }

    public MessageGroupTemplateContext(ExportContext exportContext, IReadOnlyList<Message> messages)
    {
        ExportContext = exportContext;
        Messages = messages;
    }

    public string FormatMarkdown(string? markdown, bool isJumboAllowed = true) =>
        HtmlMarkdownVisitor.Format(ExportContext, markdown ?? "", isJumboAllowed);
}