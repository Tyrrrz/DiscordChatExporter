using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
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
        // Forum channels don't have messages, they are just a list of threads
        if (request.Channel.Kind == ChannelKind.GuildForum)
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"is a forum and cannot be exported directly. "
                    + "You need to pull its threads and export them individually."
            );
        }

        var currentPartitionIndex = 0;
        // TODO: Maybe add a way to search for old files after a username change
        if (File.Exists(request.OutputFilePath))
        {
            // TODO: Maybe add an "Ask" option in the future
            switch (request.FileExistsHandling)
            {
                case FileExistsHandling.Abort:
                    Console.WriteLine("Channel aborted");
                    return;
                case FileExistsHandling.Overwrite:
                    Console.WriteLine("Removing old files");
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
                            Console.WriteLine("Download already up to date");
                            return;
                        }

                        Console.WriteLine(
                            "Downloading data after " + lastMessageSnowflake.Value.ToDate()
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
            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages; an empty file will be created."
            );
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
            throw new ChannelEmptyException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages within the specified period; an empty file will be created."
            );
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
    }
}
