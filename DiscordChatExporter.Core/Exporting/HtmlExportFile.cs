using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exceptions;

namespace DiscordChatExporter.Core.Exporting;

internal sealed partial class HtmlExportFile(
    long footerOffset,
    string footer,
    long messageCount,
    Snowflake lastMessageId
)
{
    public long FooterOffset { get; } = footerOffset;
    public string Footer { get; } = footer;
    public long MessageCount { get; } = messageCount;
    public Snowflake LastMessageId { get; } = lastMessageId;

    [GeneratedRegex(@"<div\s+id=(?:""|')?chatlog__message-container-(\d+)")]
    private static partial Regex MessageIdPattern();

    [GeneratedRegex(@"<div\s+class=(?:""chatlog""|'chatlog'|chatlog)(?:\s[^>]*)?>")]
    private static partial Regex ChatlogPattern();

    [GeneratedRegex(@"data-channel-id=(?:""|')?(\d+)(?:""|'|\s|>)")]
    private static partial Regex ChannelIdPattern();

    [GeneratedRegex(@"data-dce-complete=(?:""|')?(true|false)(?:""|'|\s|>)")]
    private static partial Regex CompletePattern();

    [GeneratedRegex(@"\n</div>\s+(?=<div class=(?:""postamble""|'postamble'|postamble)(?:\s|>))")]
    private static partial Regex FooterBoundaryPattern();

    [GeneratedRegex(@"(?<=Exported )[\d.,'\u2019\u066c\s\u00a0\u202f]+(?= message\(s\))")]
    private static partial Regex FooterCountPattern();

    public static async ValueTask<HtmlExportFile> InspectAsync(
        FileStream stream,
        string filePath,
        Snowflake channelId,
        CancellationToken cancellationToken
    )
    {
        var foundChatlog = false;
        var channelMatches = false;
        var knownIncomplete = false;
        long count = 0;
        Snowflake? last = null;
        var chronological = true;

        using (var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true))
        {
            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                var chatlogMatch = !foundChatlog ? ChatlogPattern().Match(line) : Match.Empty;
                if (chatlogMatch.Success)
                {
                    foundChatlog = true;
                    var channelMatch = ChannelIdPattern().Match(chatlogMatch.Value);
                    channelMatches = channelMatch.Success
                        ? channelMatch.Groups[1].Value == channelId.ToString()
                        : !chatlogMatch.Value.Contains("data-channel-id", StringComparison.Ordinal)
                            && Path.GetFileName(filePath)
                                .Contains($"[{channelId}]", StringComparison.Ordinal);
                    if (chatlogMatch.Value.Contains("data-dce-complete", StringComparison.Ordinal))
                    {
                        var completeMatch = CompletePattern().Match(chatlogMatch.Value);
                        knownIncomplete =
                            !completeMatch.Success || completeMatch.Groups[1].Value != "true";
                    }
                }

                if (
                    !foundChatlog
                    || !line.Contains("chatlog__message-container-", StringComparison.Ordinal)
                )
                    continue;

                foreach (Match match in MessageIdPattern().Matches(line))
                {
                    if (!ulong.TryParse(match.Groups[1].Value, out var value))
                        throw new DiscordChatExporterException(
                            "Invalid message ID in HTML export."
                        );

                    var current = new Snowflake(value);
                    if (last is not null && current.CompareTo(last.Value) <= 0)
                        chronological = false;

                    last = current;
                    count++;
                }
            }
        }

        if (!foundChatlog || !channelMatches)
        {
            throw new DiscordChatExporterException(
                "The HTML export does not identify the selected channel. For older exports, "
                    + "the file name must contain the channel ID in square brackets."
            );
        }

        if (knownIncomplete)
        {
            throw new DiscordChatExporterException(
                "The existing HTML export was filtered, partitioned, date-limited, or reversed."
            );
        }

        if (!chronological || last is null)
        {
            throw new DiscordChatExporterException(
                "Cannot append to an empty, reversed, or non-chronological HTML export."
            );
        }

        const int maxFooterBytes = 16 * 1024;
        var tailLength = (int)Math.Min(stream.Length, maxFooterBytes);
        var tailBytes = new byte[tailLength];
        stream.Position = stream.Length - tailLength;
        await stream.ReadExactlyAsync(tailBytes, cancellationToken);

        // Latin-1 maps each byte to one character, preserving byte offsets.
        var tail = Encoding.Latin1.GetString(tailBytes);
        var boundaries = FooterBoundaryPattern().Matches(tail);
        if (boundaries.Count != 1)
            throw new DiscordChatExporterException("The HTML export has an invalid footer.");

        var footerOffset = stream.Length - tailLength + boundaries[0].Index + 1;
        var footer = Encoding.UTF8.GetString(
            tailBytes,
            boundaries[0].Index + 1,
            tailLength - boundaries[0].Index - 1
        );
        var footerCounts = FooterCountPattern().Matches(footer);
        if (
            footerCounts.Count != 1
            || !long.TryParse(
                new string(footerCounts[0].Value.Where(char.IsAsciiDigit).ToArray()),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var footerCount
            )
            || footerCount != count
        )
        {
            throw new DiscordChatExporterException(
                "The HTML export's message count does not match its contents."
            );
        }

        return new HtmlExportFile(footerOffset, footer, count, last.Value);
    }

    public async ValueTask WriteUpdatedAsync(
        FileStream original,
        string fragmentPath,
        string outputPath,
        long addedMessages,
        CultureInfo? culture,
        CancellationToken cancellationToken
    )
    {
        original.Position = 0;
        await using var output = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            true
        );

        var buffer = new byte[81920];
        var remaining = FooterOffset;
        while (remaining > 0)
        {
            var read = await original.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                cancellationToken
            );
            if (read == 0)
                throw new EndOfStreamException("The original HTML export was truncated.");

            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            remaining -= read;
        }

        await using (var fragment = File.OpenRead(fragmentPath))
            await fragment.CopyToAsync(output, cancellationToken);

        var count = (MessageCount + addedMessages).ToString("n0", culture);
        var updatedFooter = FooterCountPattern().Replace(Footer, count, 1);
        await output.WriteAsync(Encoding.UTF8.GetBytes(updatedFooter), cancellationToken);
    }
}
