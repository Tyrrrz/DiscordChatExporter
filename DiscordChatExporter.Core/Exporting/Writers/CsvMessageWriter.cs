using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting.Writers;

internal partial class CsvMessageWriter : MessageWriter
{
    private readonly TextWriter _writer;

    public CsvMessageWriter(Stream stream, ExportContext context)
        : base(stream, context)
    {
        _writer = new StreamWriter(stream);
    }

    private string FormatMarkdown(string? markdown) =>
        PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

    public override async ValueTask WritePreambleAsync(CancellationToken cancellationToken = default) =>
        await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions,Embeds");

    private async ValueTask WriteAttachmentsAsync(
        IReadOnlyList<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        var buffer = new StringBuilder();

        foreach (var attachment in attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            buffer
                .AppendIfNotEmpty(',')
                .Append(await Context.ResolveMediaUrlAsync(attachment.Url, cancellationToken));
        }

        await _writer.WriteAsync(CsvEncode(buffer.ToString()));
    }

    private async ValueTask WriteReactionsAsync(
        IReadOnlyList<Reaction> reactions,
        CancellationToken cancellationToken = default)
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

    private async ValueTask WriteEmbedsAsync(
        IReadOnlyList<Embed> embeds,
        CancellationToken cancellationToken = default)
    {
        var buffer = new StringBuilder();

        foreach (var embed in embeds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            buffer.AppendIfNotEmpty('|');

            if (!string.IsNullOrWhiteSpace(embed.Author?.Name))
                buffer
                    .Append("Name: ")
                    .Append(embed.Author.Name)
                    .Append(';');

            if (!string.IsNullOrWhiteSpace(embed.Url))
                buffer
                    .Append("Url: ")
                    .Append(embed.Url)
                    .Append(';');

            if (!string.IsNullOrWhiteSpace(embed.Title))
                buffer
                    .Append("Title: ")
                    .Append(embed.Title)
                    .Append(';');

            if (!string.IsNullOrWhiteSpace(embed.Description))
                buffer
                    .Append("Description: ")
                    .Append(embed.Description)
                    .Append(';');

            foreach (var field in embed.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Name))
                    buffer
                        .Append("Field: ")
                        .Append(field.Name);

                if (!string.IsNullOrWhiteSpace(field.Value))
                    buffer
                        .Append("=")
                        .Append(field.Value)
                        .Append(';');
            }

            if (!string.IsNullOrWhiteSpace(embed.Thumbnail?.Url))
                buffer
                    .Append("Thumbnail: ")
                    .Append(await Context.ResolveMediaUrlAsync(embed.Thumbnail.ProxyUrl ?? embed.Thumbnail.Url, cancellationToken))
                    .Append(';');

            if (!string.IsNullOrWhiteSpace(embed.Image?.Url))
                buffer
                    .Append("Image: ")
                    .Append(await Context.ResolveMediaUrlAsync(embed.Image.ProxyUrl ?? embed.Image.Url, cancellationToken))
                    .Append(';');

            if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
                buffer
                    .Append("Footer: ")
                    .Append(embed.Footer.Text)
                    .Append(';');
        }

        await _writer.WriteAsync(CsvEncode(buffer.ToString()));
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // Author ID
        await _writer.WriteAsync(CsvEncode(message.Author.Id.ToString()));
        await _writer.WriteAsync(',');

        // Author name
        await _writer.WriteAsync(CsvEncode(message.Author.FullName));
        await _writer.WriteAsync(',');

        // Message timestamp
        await _writer.WriteAsync(CsvEncode(Context.FormatDate(message.Timestamp)));
        await _writer.WriteAsync(',');

        // Message content
        await _writer.WriteAsync(CsvEncode(FormatMarkdown(message.Content)));
        await _writer.WriteAsync(',');

        // Attachments
        await WriteAttachmentsAsync(message.Attachments, cancellationToken);
        await _writer.WriteAsync(',');

        // Reactions
        await WriteReactionsAsync(message.Reactions, cancellationToken);
        await _writer.WriteAsync(',');

        // Embeds
        await WriteEmbedsAsync(message.Embeds, cancellationToken);

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
        value = value.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }
}