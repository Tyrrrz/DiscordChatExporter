using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Initialize the exporter before further checks to ensure the file is created even if
        // an exception is thrown after this point.
        await using var messageExporter = new MessageExporter(context);

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

        var messages = !request.IsReverseMessageOrder
            ? discord.GetMessagesAsync(
                request.Channel.Id,
                request.After,
                request.Before,
                progress,
                cancellationToken
            )
            : discord.GetMessagesInReverseAsync(
                request.Channel.Id,
                request.After,
                request.Before,
                progress,
                cancellationToken
            );

        // Threads created from a message don't include that message in their own history, even
        // though it logically belongs to the thread as its very first message. Because the thread
        // shares its ID with that starter message, we can fetch it from the parent channel and
        // export it together with the rest of the thread's messages.
        // This doesn't apply to forum/media posts, whose starter message is already part of the
        // thread's own history.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1265
        var starterMessage =
            request.Channel.IsThread
            && request.Channel.Parent is { Kind: not ChannelKind.GuildForum } parent
                ? await discord.TryGetMessageAsync(parent.Id, request.Channel.Id, cancellationToken)
                : null;

        // Only include the starter message if it falls within the requested range
        if (
            starterMessage is not null
            && (
                (request.After is not null && starterMessage.Id < request.After.Value)
                || (request.Before is not null && starterMessage.Id > request.Before.Value)
            )
        )
        {
            starterMessage = null;
        }

        // Prepend the starter message to the thread's history, or append it when exporting in
        // reverse order, so that it remains the oldest message in the output either way.
        async IAsyncEnumerable<Message> GetMessagesWithStarter(
            [EnumeratorCancellation] CancellationToken innerCancellationToken
        )
        {
            if (starterMessage is not null && !request.IsReverseMessageOrder)
                yield return starterMessage;

            await foreach (var message in messages.WithCancellation(innerCancellationToken))
            {
                // Avoid exporting the starter message twice in case it's also returned as part
                // of the thread's own history.
                if (message.Id != starterMessage?.Id)
                    yield return message;
            }

            if (starterMessage is not null && request.IsReverseMessageOrder)
                yield return starterMessage;
        }

        await foreach (var message in GetMessagesWithStarter(cancellationToken))
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
