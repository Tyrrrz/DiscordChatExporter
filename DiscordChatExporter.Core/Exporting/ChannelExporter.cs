using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
        ConcurrentDictionary<string, string[]> outputDirFilesDict,
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

        if (
            !DetectExistingExport(
                request,
                logger,
                request.SearchForExistingExports,
                outputDirFilesDict,
                out var existingExportFile
            )
        )
            return;
        if (
            !HandleExistingExport(
                request,
                logger,
                existingExportFile,
                out var currentPartitionIndex
            )
        )
            return;

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Initialize the exporter before further checks to ensure the file is created even if
        // an exception is thrown after this point.
        await using var messageExporter = new MessageExporter(
            context,
            currentPartitionIndex!.Value
        );

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

        if (existingExportFile == null)
        {
            logger.IncrementCounter(ExportResult.NewExportSuccess);
            if (logSuccess)
                logger.LogSuccess(request, "Successfully exported the channel");
        }
        else if (request.ExportExistsHandling == ExportExistsHandling.Append)
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

    /// <summary>
    /// Detects whether an existing export of the given request exists.
    /// </summary>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="logger">The logger that's used to log progress updates about the export.</param>
    /// <param name="searchForExistingExport">
    /// If false, it will only be detected whether an existing export exists at the current target file path.
    /// This means that an existing export won't be detected if the name of the channel, the channel parent or the
    /// guild has changed or if the default file name formatting has changed.
    /// If true, the entire directory will be searched for an existing export file of the given request (if there is
    /// none at the current target file path).
    /// </param>
    /// <param name="outputDirFilesDict">
    /// A thread-safe dictionary mapping lists of filenames to the directory they're in.
    /// If the directory files are needed, they will be collected lazily and stored for future use in other channel
    /// exports.
    /// </param>
    /// <param name="existingExportFile">
    /// The absolute base file path of the existing export of this request, if one has been detected. Null otherwise.
    /// </param>
    /// <returns>
    /// Whether the export should continue normally (true) or return (false).
    /// This is true if no, or exactly one, existing export of this request has been detected, and false if
    /// several ones have been detected.
    /// </returns>
    private static bool DetectExistingExport(
        ExportRequest request,
        ProgressLogger logger,
        bool searchForExistingExport,
        ConcurrentDictionary<string, string[]> outputDirFilesDict,
        out string? existingExportFile
    )
    {
        existingExportFile = null;

        if (File.Exists(request.OutputFilePath))
        {
            existingExportFile = request.OutputFilePath;
            return true;
        }
        if (!searchForExistingExport)
            return true;

        // Look for an existing export under a different file name
        var outputFileRegex = request.GetDefaultOutputFileNameRegex();
        var outputDirFiles = outputDirFilesDict.GetOrAdd(
            request.OutputDirPath,
            outputDirPath =>
                Directory
                    .GetFiles(outputDirPath)
                    .Select(Path.GetFileName)
                    .Select(fileName => fileName!)
                    .ToArray()
        );
        var regexFiles = outputDirFiles
            .Where(fileName => outputFileRegex.IsMatch(fileName))
            .ToArray();
        if (regexFiles.Length == 0)
            return true;
        if (regexFiles.Length > 1)
        {
            logger.LogError(
                request,
                "Found multiple existing channel exports under different file names: "
                    + string.Join(", ", regexFiles.Select(fileName => $"\"{fileName}\""))
                    + "."
            );
            return false;
        }

        logger.LogInfo(
            request,
            $"Found existing channel export under file name \"{regexFiles[0]}\"."
        );
        existingExportFile = Path.Combine(request.OutputDirPath, regexFiles[0]);
        return true;
    }

    /// <summary>
    /// Handles the existing export files of the current request according to the set file exists handling.
    /// </summary>
    /// <param name="request">The request specifying the current channel export.</param>
    /// <param name="logger">The logger that's used to log progress updates about the export.</param>
    /// <param name="existingExportFile">
    /// The absolute base file path of the existing export of this request, if one has been detected.
    /// If this is null, the function will immediately return.
    /// </param>
    /// <param name="currentPartitionIndex">
    /// The index of the current export partition the newly exported messages should be written to.
    /// </param>
    /// <returns>
    /// Whether the export should continue normally (true) or return (false).
    /// This is false both if the export should be aborted and if the export is already up to date, and true otherwise.
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static bool HandleExistingExport(
        ExportRequest request,
        ProgressLogger logger,
        string? existingExportFile,
        out int? currentPartitionIndex
    )
    {
        currentPartitionIndex = null;

        if (existingExportFile == null)
        {
            currentPartitionIndex = 0;
            return true;
        }

        // TODO: Maybe add an "Ask" option in the future
        switch (request.ExportExistsHandling)
        {
            case ExportExistsHandling.Abort:
                logger.LogError(request, "Aborted export due to existing export files");
                return false;
            case ExportExistsHandling.Overwrite:
                logger.LogWarning(request, "Removing existing export files");
                MessageExporter.RemoveExistingExport(existingExportFile);
                currentPartitionIndex = 0;
                return true;
            case ExportExistsHandling.Append:
                if (existingExportFile != request.OutputFilePath)
                {
                    logger.LogInfo(request, "Moving existing export files to the new file names");
                    MessageExporter.MoveExistingExport(existingExportFile, request.OutputFilePath);
                }

                var lastMessageSnowflake = MessageExporter.GetLastMessageSnowflake(
                    request.OutputFilePath,
                    request.Format
                );
                if (lastMessageSnowflake != null)
                {
                    if (!request.Channel.MayHaveMessagesAfter(lastMessageSnowflake.Value))
                    {
                        logger.IncrementCounter(ExportResult.UpdateExportSkip);
                        logger.LogInfo(request, "Existing export already up to date");
                        return false;
                    }
                    request.LastPriorMessage = lastMessageSnowflake.Value;
                    logger.LogInfo(
                        request,
                        "Appending existing export starting at "
                            + lastMessageSnowflake.Value.ToDate()
                    );
                }
                else
                {
                    if (request.Channel.IsEmpty)
                    {
                        logger.IncrementCounter(ExportResult.UpdateExportSkip);
                        logger.LogInfo(request, "Existing empty export already up to date");
                        return false;
                    }
                    logger.LogInfo(request, "Appending existing empty export.");
                }

                currentPartitionIndex = MessageExporter.GetPartitionCount(request.OutputFilePath);
                return true;
            default:
                throw new InvalidOperationException(
                    $"Unknown FileExistsHandling value '{request.ExportExistsHandling}'."
                );
        }
    }
}
