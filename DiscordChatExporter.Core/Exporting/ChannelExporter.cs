using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting.Logging;
using Gress;

namespace DiscordChatExporter.Core.Exporting;

public class ChannelExporter(DiscordClient discord)
{
    public async ValueTask ExportChannelAsync(
        ProgressLogger logger,
        bool logSuccess,
        ExportRequest request,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Forum channels don't have messages, they are just a list of threads
        if (request.Channel.Kind == ChannelKind.GuildForum)
        {
            // TODO: The GUI apparently has no thread inclusion setting
            logger.LogError(
                request,
                "This channel is a forum and cannot be exported. "
                    + "Did you forget to turn on thread inclusion?"
            );
            return;
        }

        var currentPartitionIndex = 0;
        var exportExists = false;
        // TODO: Maybe add a way to search for old files after a username change
        if (File.Exists(request.OutputFilePath))
        {
            exportExists = true;
            // TODO: Maybe add an "Ask" option in the future
            switch (request.FileExistsHandling)
            {
                case FileExistsHandling.Abort:
                    logger.LogError(request, "Aborted export due to existing export files");
                    return;
                case FileExistsHandling.Overwrite:
                    logger.LogWarning(request, "Removing existing export files");
                    MessageExporter.RemoveExistingFiles(request.OutputFilePath);
                    break;
                case FileExistsHandling.Append:
                    var lastMessageSnowflake = MessageExporter.GetLastMessageSnowflake(
                        request.OutputFilePath,
                        request.Format
                    );
                    if (lastMessageSnowflake != null)
                    {
                        request.LastPriorMessage = lastMessageSnowflake.Value;

                        if (!request.Channel.MayHaveMessagesAfter(request.LastPriorMessage.Value))
                        {
                            logger.IncrementCounter(ExportResult.UpdateExportSkip);
                            logger.LogInfo(request, "Existing export already up to date");
                            return;
                        }

                        logger.LogInfo(
                            request,
                            "Appending existing export starting at "
                                + lastMessageSnowflake.Value.ToDate()
                        );
                        currentPartitionIndex = MessageExporter.GetPartitionCount(
                            request.OutputFilePath
                        );
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown FileExistsHandling value '{request.FileExistsHandling}'."
                    );
            }
        }

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Initialize the exporter before further checks to ensure the file is created even if
        // an exception is thrown after this point.
        await using var messageExporter = new MessageExporter(context, currentPartitionIndex);

        // Check if the channel is empty
        if (request.Channel.IsEmpty)
        {
            logger.IncrementCounter(ExportResult.NewExportSuccess);
            logger.IncrementCounter(ExportResult.NewExportSuccessEmpty);
            logger.LogInfo(request, "The channel does not contain any messages");
            return;
        }

        // Check if the 'before' and 'after' boundaries are valid
        if (
            (
                request.Before is not null
                && !request.Channel.MayHaveMessagesBefore(request.Before.Value)
            )
            || (
                request.After is not null
                && !request.Channel.MayHaveMessagesAfter(request.After.Value)
            )
        )
        {
            logger.IncrementCounter(ExportResult.NewExportSuccess);
            logger.IncrementCounter(ExportResult.NewExportSuccessEmpty);
            logger.LogWarning(
                request,
                "The channel does not contain any messages within the specified period"
            );
            return;
        }

        await foreach (
            var message in discord.GetMessagesAsync(
                request.Channel.Id,
                request.LastPriorMessage ?? request.After,
                request.Before,
                progress,
                cancellationToken
            )
        )
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

        if (!exportExists)
        {
            logger.IncrementCounter(ExportResult.NewExportSuccess);
            if (logSuccess)
                logger.LogSuccess(request, "Successfully exported the channel");
        }
        else if (request.FileExistsHandling == FileExistsHandling.Append)
        {
            logger.IncrementCounter(ExportResult.UpdateExportSuccess);
            if (logSuccess)
                logger.LogSuccess(request, "Successfully appended the channel export");
        }
        else
        {
            logger.IncrementCounter(ExportResult.UpdateExportSuccess);
            if (logSuccess)
                logger.LogSuccess(request, "Successfully overwrote the channel export");
        }
    }
}
