using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Writers.Html;

namespace DiscordChatExporter.Core.Exporting.Writers;

internal class HtmlMessageWriter : MessageWriter
{
    private readonly TextWriter _writer;
    private readonly string _themeName;

    private readonly List<Message> _messageGroupBuffer = new();

    public HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
        : base(stream, context)
    {
        _writer = new StreamWriter(stream);
        _themeName = themeName;
    }

    public override async ValueTask WritePreambleAsync(CancellationToken cancellationToken = default)
    {
        var templateContext = new PreambleTemplateContext(Context, _themeName);

        // We are not writing directly to output because Razor
        // does not actually do asynchronous writes to stream.
        await _writer.WriteLineAsync(
            await PreambleTemplate.RenderAsync(templateContext, cancellationToken)
        );
    }

    private async ValueTask WriteMessageGroupAsync(
        MessageGroup messageGroup,
        CancellationToken cancellationToken = default)
    {
        var templateContext = new MessageGroupTemplateContext(Context, messageGroup);

        // We are not writing directly to output because Razor
        // does not actually do asynchronous writes to stream.
        await _writer.WriteLineAsync(
            await MessageGroupTemplate.RenderAsync(templateContext, cancellationToken)
        );
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // If message group is empty or the given message can be grouped, buffer the given message
        if (!_messageGroupBuffer.Any() || MessageGroup.CanJoin(_messageGroupBuffer.Last(), message))
        {
            _messageGroupBuffer.Add(message);
        }
        // Otherwise, flush the group and render messages
        else
        {
            await WriteMessageGroupAsync(MessageGroup.Join(_messageGroupBuffer), cancellationToken);

            _messageGroupBuffer.Clear();
            _messageGroupBuffer.Add(message);
        }
    }

    public override async ValueTask WritePostambleAsync(CancellationToken cancellationToken = default)
    {
        // Flush current message group
        if (_messageGroupBuffer.Any())
        {
            await WriteMessageGroupAsync(
                MessageGroup.Join(_messageGroupBuffer),
                cancellationToken
            );
        }

        var templateContext = new PostambleTemplateContext(Context, MessagesWritten);

        // We are not writing directly to output because Razor
        // does not actually do asynchronous writes to stream.
        await _writer.WriteLineAsync(
            await PostambleTemplate.RenderAsync(templateContext, cancellationToken)
        );
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }
}