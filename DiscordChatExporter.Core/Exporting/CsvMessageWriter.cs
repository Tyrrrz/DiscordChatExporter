using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class CsvMessageWriter(Stream stream, ExportContext context)
    : MessageWriter(stream, context)
{
    private readonly TextWriter _writer = new StreamWriter(stream);

    private async ValueTask<string> FormatMarkdownAsync(
        string markdown,
        CancellationToken cancellationToken = default
    ) =>
        Context.Request.ShouldFormatMarkdown
            ? await PlainTextMarkdownVisitor.FormatAsync(Context, markdown, cancellationToken)
            : markdown;

    public override async ValueTask WritePreambleAsync(
        CancellationToken cancellationToken = default
    ) => await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

    private async ValueTask WriteAttachmentsAsync(
        IReadOnlyList<Attachment> attachments,
        CancellationToken cancellationToken = default
    )
    {
        var buffer = new StringBuilder();

        foreach (var attachment in attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            buffer
                .AppendIfNotEmpty(',')
                .Append(await Context.ResolveAssetUrlAsync(attachment.Url, cancellationToken));
        }

        await _writer.WriteAsync(CsvEncode(buffer.ToString()));
    }

    private async ValueTask WriteReactionsAsync(
        IReadOnlyList<Reaction> reactions,
        CancellationToken cancellationToken = default
    )
    {
        var buffer = new StringBuilder();

        foreach (var reaction in reactions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            buffer
                .AppendIfNotEmpty(',')
                .Append(reaction.Emoji.Name)
                .Append(' ')
                .Append('(')
                .Append(reaction.Count)
                .Append(')');
        }

        await _writer.WriteAsync(CsvEncode(buffer.ToString()));
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // Author ID
        await _writer.WriteAsync(CsvEncode(message.Author.Id.ToString()));
        await _writer.WriteAsync(',');

        // Author name
        await _writer.WriteAsync(CsvEncode(message.Author.FullName));
        await _writer.WriteAsync(',');

        // Message timestamp
        await _writer.WriteAsync(CsvEncode(Context.FormatDate(message.Timestamp, "o")));
        await _writer.WriteAsync(',');

        // Message content
        if (message.IsSystemNotification)
        {
            await _writer.WriteAsync(CsvEncode(message.GetFallbackContent()));
        }
        else
        {
            await _writer.WriteAsync(
                CsvEncode(await FormatMarkdownAsync(message.Content, cancellationToken))
            );
        }

        await _writer.WriteAsync(',');

        // Attachments
        await WriteAttachmentsAsync(message.Attachments, cancellationToken);
        await _writer.WriteAsync(',');

        // Reactions
        await WriteReactionsAsync(message.Reactions, cancellationToken);

        // Finish row
        await _writer.WriteLineAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }
}

internal partial class CsvMessageWriter
{
    private static string CsvEncode(string value)
    {
        value = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{value}\"";
    }
}
