using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using Gress;

namespace DiscordChatExporter.Core.Exporting;

public class ChannelExporter(DiscordClient discord)
{
    public async ValueTask ExportChannelAsync(
        ExportRequest request,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        if (request.ShouldAppendToHtml)
        {
            await UpdateHtmlAsync(request, progress, cancellationToken);
            return;
        }

        await ExportToFileAsync(
            request,
            request.Channel,
            request.After,
            null,
            false,
            progress,
            cancellationToken
        );
    }

    private async ValueTask<long> ExportToFileAsync(
        ExportRequest request,
        Channel channel,
        Snowflake? after,
        string? outputFilePath,
        bool htmlFragment,
        IProgress<Percentage>? progress,
        CancellationToken cancellationToken
    )
    {
        // Forum channels don't have messages, they are just a list of threads
        if (channel.Kind == ChannelKind.GuildForum)
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"is a forum and cannot be exported directly. "
                    + "You need to pull its threads and export them individually."
            );
        }

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Initialize the exporter before further checks to ensure the file is created even if
        // an exception is thrown after this point.
        await using var messageExporter = new MessageExporter(
            context,
            outputFilePath,
            htmlFragment
        );

        // Check if the channel is empty
        if (channel.IsEmpty)
        {
            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages; an empty file will be created."
            );
        }

        // Check if the 'before' and 'after' boundaries are valid
        if (
            (request.Before is not null && !channel.MayHaveMessagesBefore(request.Before.Value))
            || (after is not null && !channel.MayHaveMessagesAfter(after.Value))
        )
        {
            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages within the specified period; an empty file will be created."
            );
        }

        var messages =
            request.ShouldAppendToHtml
                ? discord.GetMessagesForUpdateAsync(
                    request.Channel.Id,
                    after,
                    progress,
                    cancellationToken
                )
            : !request.IsReverseMessageOrder
                ? discord.GetMessagesAsync(
                    request.Channel.Id,
                    after,
                    request.Before,
                    progress,
                    cancellationToken
                )
            : discord.GetMessagesInReverseAsync(
                request.Channel.Id,
                after,
                request.Before,
                progress,
                cancellationToken
            );

        await foreach (var message in messages)
        {
            try
            {
                // Resolve members for referenced users
                foreach (var user in message.GetReferencedUsers())
                    await context.PopulateMemberAsync(user, cancellationToken);

                // Export the message
                if (request.MessageFilter.IsMatch(message))
                    await messageExporter.ExportMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                // Provide more context to the exception, to simplify debugging based on error messages
                throw new DiscordChatExporterException(
                    $"Failed to export message #{message.Id} "
                        + $"in channel '{request.Channel.Name}' (#{request.Channel.Id}) "
                        + $"of guild '{request.Guild.Name} (#{request.Guild.Id})'.",
                    ex is not DiscordChatExporterException dex || dex.IsFatal,
                    ex
                );
            }
        }

        return messageExporter.MessagesExported;
    }

    private async ValueTask UpdateHtmlAsync(
        ExportRequest request,
        IProgress<Percentage>? progress,
        CancellationToken cancellationToken
    )
    {
        if (
            request.Format is not (ExportFormat.HtmlDark or ExportFormat.HtmlLight)
            || request.After is not null
            || request.Before is not null
            || request.PartitionLimit != PartitionLimit.Null
            || request.MessageFilter != MessageFilter.Null
            || request.IsReverseMessageOrder
            || !string.Equals(
                Path.GetExtension(request.OutputFilePath),
                ".html",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new DiscordChatExporterException(
                "HTML updates require an existing HTML file and cannot be combined with date "
                    + "limits, filters, partitions, or reverse order."
            );
        }

        if (request.Channel.Kind == ChannelKind.GuildForum)
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"is a forum and cannot be exported directly. "
                    + "You need to pull its threads and export them individually."
            );
        }

        if (request.Channel.IsThread)
            throw new DiscordChatExporterException("Appending to thread exports is not supported.");

        var path = request.OutputFilePath;
        if (!File.Exists(path))
        {
            throw new DiscordChatExporterException(
                $"The HTML export file '{path}' does not exist."
            );
        }

        var baseName = Path.GetFileNameWithoutExtension(path);
        var secondPart = Path.Combine(
            request.OutputDirPath,
            $"{baseName} [part 2]{Path.GetExtension(path)}"
        );
        if (Regex.IsMatch(baseName, @" \[part \d+\]$") || File.Exists(secondPart))
            throw new DiscordChatExporterException(
                "Updating partitioned HTML exports is not supported."
            );

        var temporaryPath = Path.Combine(
            request.OutputDirPath,
            $".dce-update-{Guid.NewGuid():N}.tmp"
        );
        var fragmentPath = Path.Combine(
            request.OutputDirPath,
            $".dce-fragment-{Guid.NewGuid():N}.tmp"
        );
        var backupPath = $"{path}.dce-backup-{Guid.NewGuid():N}";

        try
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(path);
            long originalLength;

            await using (
                var original = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    true
                )
            )
            {
                originalLength = original.Length;
                var existing = await HtmlExportFile.InspectAsync(
                    original,
                    path,
                    request.Channel.Id,
                    cancellationToken
                );

                var channel = await discord.GetChannelAsync(request.Channel.Id, cancellationToken);
                if (channel.IsEmpty)
                {
                    throw new DiscordChatExporterException(
                        "The channel has no accessible last message; the existing export was not replaced."
                    );
                }

                if (!channel.MayHaveMessagesAfter(existing.LastMessageId))
                    return;

                var added = await ExportToFileAsync(
                    request,
                    channel,
                    existing.LastMessageId,
                    fragmentPath,
                    true,
                    progress,
                    cancellationToken
                );

                if (added == 0)
                    return;

                await existing.WriteUpdatedAsync(
                    original,
                    fragmentPath,
                    temporaryPath,
                    added,
                    request.CultureInfo,
                    cancellationToken
                );
            }

            cancellationToken.ThrowIfCancellationRequested();
            var currentFile = new FileInfo(path);
            if (
                !currentFile.Exists
                || currentFile.Length != originalLength
                || currentFile.LastWriteTimeUtc != lastWriteTime
            )
            {
                throw new DiscordChatExporterException(
                    "The existing export changed during the update."
                );
            }

            try
            {
                File.Replace(temporaryPath, path, backupPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new DiscordChatExporterException(
                    $"Failed to replace the existing HTML export '{path}'.",
                    false,
                    ex
                );
            }
        }
        finally
        {
            TryDeleteFile(temporaryPath);
            TryDeleteFile(fragmentPath);
        }

        static void TryDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
