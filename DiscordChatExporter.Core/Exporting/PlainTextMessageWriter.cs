using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class PlainTextMessageWriter(Stream stream, ExportContext context)
    : MessageWriter(stream, context)
{
    private const int HeaderSize = 4;
    private const int FooterSize = 3;

    private readonly TextWriter _writer = new StreamWriter(stream);

    private async ValueTask<string> FormatMarkdownAsync(
        string markdown,
        CancellationToken cancellationToken = default
    ) =>
        Context.Request.ShouldFormatMarkdown
            ? await PlainTextMarkdownVisitor.FormatAsync(Context, markdown, cancellationToken)
            : markdown;

    private async ValueTask WriteMessageHeaderAsync(Message message)
    {
        // Timestamp & author
        await _writer.WriteAsync($"[{Context.FormatDate(message.Timestamp)}]");
        await _writer.WriteAsync($" {message.Author.FullName}");

        // Whether the message is pinned
        if (message.IsPinned)
            await _writer.WriteAsync(" (pinned)");

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteAttachmentsAsync(
        IReadOnlyList<Attachment> attachments,
        CancellationToken cancellationToken = default
    )
    {
        if (!attachments.Any())
            return;

        await _writer.WriteLineAsync("{Attachments}");

        foreach (var attachment in attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync(
                await Context.ResolveAssetUrlAsync(attachment.Url, cancellationToken)
            );
        }

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteEmbedsAsync(
        IReadOnlyList<Embed> embeds,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var embed in embeds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync("{Embed}");

            if (!string.IsNullOrWhiteSpace(embed.Author?.Name))
            {
                await _writer.WriteLineAsync(embed.Author.Name);
            }

            if (!string.IsNullOrWhiteSpace(embed.Url))
            {
                await _writer.WriteLineAsync(embed.Url);
            }

            if (!string.IsNullOrWhiteSpace(embed.Title))
            {
                await _writer.WriteLineAsync(
                    await FormatMarkdownAsync(embed.Title, cancellationToken)
                );
            }

            if (!string.IsNullOrWhiteSpace(embed.Description))
            {
                await _writer.WriteLineAsync(
                    await FormatMarkdownAsync(embed.Description, cancellationToken)
                );
            }

            foreach (var field in embed.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Name))
                {
                    await _writer.WriteLineAsync(
                        await FormatMarkdownAsync(field.Name, cancellationToken)
                    );
                }

                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    await _writer.WriteLineAsync(
                        await FormatMarkdownAsync(field.Value, cancellationToken)
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(embed.Thumbnail?.Url))
            {
                await _writer.WriteLineAsync(
                    await Context.ResolveAssetUrlAsync(
                        embed.Thumbnail.ProxyUrl ?? embed.Thumbnail.Url,
                        cancellationToken
                    )
                );
            }

            foreach (var image in embed.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.Url))
                {
                    await _writer.WriteLineAsync(
                        await Context.ResolveAssetUrlAsync(
                            image.ProxyUrl ?? image.Url,
                            cancellationToken
                        )
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
            {
                await _writer.WriteLineAsync(embed.Footer.Text);
            }

            await _writer.WriteLineAsync();
        }
    }

    private async ValueTask WriteStickersAsync(
        IReadOnlyList<Sticker> stickers,
        CancellationToken cancellationToken = default
    )
    {
        if (!stickers.Any())
            return;

        await _writer.WriteLineAsync("{Stickers}");

        foreach (var sticker in stickers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _writer.WriteLineAsync(
                await Context.ResolveAssetUrlAsync(sticker.SourceUrl, cancellationToken)
            );
        }

        await _writer.WriteLineAsync();
    }

    private async ValueTask WriteReactionsAsync(
        IReadOnlyList<Reaction> reactions,
        CancellationToken cancellationToken = default
    )
    {
        if (!reactions.Any())
            return;

        await _writer.WriteLineAsync("{Reactions}");

        foreach (var (reaction, i) in reactions.WithIndex())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (i > 0)
            {
                await _writer.WriteAsync(' ');
            }

            await _writer.WriteAsync(reaction.Emoji.Name);

            if (reaction.Count > 1)
            {
                await _writer.WriteAsync($" ({reaction.Count})");
            }
        }

        await _writer.WriteLineAsync();
    }

    public override async ValueTask WritePreambleAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync($"Guild: {Context.Request.Guild.Name}");
        await _writer.WriteLineAsync($"Channel: {Context.Request.Channel.GetHierarchicalName()}");

        if (!string.IsNullOrWhiteSpace(Context.Request.Channel.Topic))
        {
            await _writer.WriteLineAsync($"Topic: {Context.Request.Channel.Topic}");
        }

        if (Context.Request.After is not null)
        {
            await _writer.WriteLineAsync(
                $"After: {Context.FormatDate(Context.Request.After.Value.ToDate())}"
            );
        }

        if (Context.Request.Before is not null)
        {
            await _writer.WriteLineAsync(
                $"Before: {Context.FormatDate(Context.Request.Before.Value.ToDate())}"
            );
        }

        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync();
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // Header
        await WriteMessageHeaderAsync(message);

        // Content
        if (message.IsSystemNotification)
        {
            await _writer.WriteLineAsync(message.GetFallbackContent());
        }
        else
        {
            await _writer.WriteLineAsync(
                await FormatMarkdownAsync(message.Content, cancellationToken)
            );
        }

        await _writer.WriteLineAsync();

        // Attachments, embeds, reactions, etc.
        await WriteAttachmentsAsync(message.Attachments, cancellationToken);
        await WriteEmbedsAsync(message.Embeds, cancellationToken);
        await WriteStickersAsync(message.Stickers, cancellationToken);
        await WriteReactionsAsync(message.Reactions, cancellationToken);

        await _writer.WriteLineAsync();
    }

    public override async ValueTask WritePostambleAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _writer.WriteLineAsync(new string('=', 62));
        await _writer.WriteLineAsync($"Exported {MessagesWritten:N0} message(s)");
        await _writer.WriteLineAsync(new string('=', 62));
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Returns the statically created regex that detects and captures the timestamp of a message in a channel TXT
    /// export.
    /// </summary>
    /// <returns>
    /// The regex that detects and captures the timestamp of a message in a Discord channel TXT export.
    /// </returns>
    [GeneratedRegex(@"^\[(.*)\] .*")]
    private static partial Regex MessageDateRegex();

    /// <summary>
    /// Retrieves and returns the approximate timestamp of the last written message in the Discord channel that has
    /// been exported with the PlainTextMessageWriter to the given file path as a Snowflake.
    /// This timestamp only has minute-level precision.
    /// </summary>
    /// <param name="filePath">
    /// The path of the Discord channel TXT export whose last message's timestamp should be returned.
    /// </param>
    /// <returns>
    /// The approximate timestamp of the last written message in the Discord channel TXT export under the given path
    /// as a Snowflake.
    /// Null, if the Discord channel TXT export doesn't include any message.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the file at the given path isn't a correctly formatted Discord channel TXT export.
    /// </exception>
    public static Snowflake? GetLastMessageDate(string filePath)
    {
        var fileLines = File.ReadAllLines(filePath);
        var fileContainsMessages = fileLines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Skip(HeaderSize + FooterSize)
            .Any();
        if (!fileContainsMessages)
            return null;

        var messageDateRegex = MessageDateRegex();

        // Find the last line with a message timestamp
        for (var i = fileLines.Length - 1 - FooterSize; i >= HeaderSize; i--)
        {
            var timestampMatch = messageDateRegex.Match(fileLines[i]);
            if (!timestampMatch.Success)
                continue;

            var timestampString = timestampMatch.Groups[1].Value;
            if (
                DateTimeOffset.TryParse(timestampString, out var timestamp)
                && fileLines[i - 1] == ""
            )
            {
                return Snowflake.FromDate(timestamp, true);
            }
        }

        throw new FormatException(
            "The TXT file is not correctly formatted; the last message timestamp could not be retrieved."
        );
    }
}
