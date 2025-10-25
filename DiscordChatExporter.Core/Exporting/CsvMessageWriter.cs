using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class CsvMessageWriter(Stream stream, ExportContext context)
    : MessageWriter(stream, context)
{
    private const int HeaderSize = 1;

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

    /// <summary>
    /// Retrieves and returns the timestamp of the last written message in the Discord channel that has been exported
    /// with the CsvMessageWriter to the given file path as a Snowflake.
    /// This timestamp has millisecond-level precision.
    /// </summary>
    /// <param name="filePath">
    /// The path of the Discord channel CSV export whose last message's timestamp should be returned.
    /// </param>
    /// <returns>
    /// The timestamp of the last written message in the Discord channel CSV export under the given path as a Snowflake.
    /// Null, if the Discord channel CSV export doesn't include any message.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the file at the given path isn't a correctly formatted Discord channel CSV export.
    /// </exception>
    public static Snowflake? GetLastMessageDate(string filePath)
    {
        try
        {
            var fileLines = File.ReadAllLines(filePath)
                .SkipWhile(string.IsNullOrWhiteSpace)
                .ToArray();
            if (fileLines.Length <= HeaderSize)
                return null;

            const string columnPattern = "(?:[^\"]?(?:\"\")?)*";
            var messageDatePattern = string.Format(
                "^\"{0}\",\"{0}\",\"({0})\",\"{0}\",\"{0}\",\"{0}\"$",
                columnPattern
            );
            var messageDateRegex = new Regex(messageDatePattern);

            var timestampMatch = messageDateRegex.Match(fileLines[^1]);
            var timestampString = timestampMatch.Groups[1].Value;
            var timestamp = DateTimeOffset.Parse(timestampString);
            return Snowflake.FromDate(timestamp, true);
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException or FormatException)
        {
            throw new FormatException(
                "The CSV file is not correctly formatted; the last message timestamp could not be retrieved."
            );
        }
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
