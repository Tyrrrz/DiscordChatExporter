using System;
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

    private readonly List<Message> _messageGroup = new();

    public HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
        : base(stream, context)
    {
        _writer = new StreamWriter(stream);
        _themeName = themeName;
    }

    private bool CanJoinGroup(Message message)
    {
        var lastMessage = _messageGroup.LastOrDefault();
        if (lastMessage is null)
            return true;

        return
            // Must be from the same author
            lastMessage.Author.Id == message.Author.Id &&
            // Author's name must not have changed between messages
            string.Equals(lastMessage.Author.FullName, message.Author.FullName, StringComparison.Ordinal) &&
            // Duration between messages must be 7 minutes or less
            (message.Timestamp - lastMessage.Timestamp).Duration().TotalMinutes <= 7 &&
            // Other message must not be a reply
            message.Reference is null;
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
        IReadOnlyList<Message> messages,
        CancellationToken cancellationToken = default)
    {
        var templateContext = new MessageGroupTemplateContext(Context, messages);

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

        // If the message can be grouped, buffer it for now
        if (CanJoinGroup( message))
        {
            _messageGroup.Add(message);
        }
        // Otherwise, flush the group and render messages
        else
        {
            await WriteMessageGroupAsync(_messageGroup, cancellationToken);

            _messageGroup.Clear();
            _messageGroup.Add(message);
        }
    }

    public override async ValueTask WritePostambleAsync(CancellationToken cancellationToken = default)
    {
        // Flush current message group
        if (_messageGroup.Any())
            await WriteMessageGroupAsync(_messageGroup, cancellationToken);

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