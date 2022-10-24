using System.Collections.Generic;
using System.Threading.Tasks;
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

    public ValueTask<string> FormatMarkdownAsync(string? markdown, bool isJumboAllowed = true) => 
        HtmlMarkdownVisitor.FormatAsync(ExportContext, markdown ?? "", isJumboAllowed);
}