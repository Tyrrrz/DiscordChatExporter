using System;
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

        // Check if the channel is empty
        if (request.Channel.IsEmpty)
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages."
            );
        }

        // Check if the 'after' boundary is valid
        if (request.After is not null && !request.Channel.MayHaveMessagesAfter(request.After.Value))
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages within the specified period."
            );
        }

        // Check if the 'before' boundary is valid
        if (
            request.Before is not null
            && !request.Channel.MayHaveMessagesBefore(request.Before.Value)
        )
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' "
                    + $"of guild '{request.Guild.Name}' "
                    + $"does not contain any messages within the specified period."
            );
        }

        // Build context
        var context = new ExportContext(discord, request);
        await context.PopulateChannelsAndRolesAsync(cancellationToken);

        // Export messages
        await using var messageExporter = new MessageExporter(context);
        await foreach (
            var message in discord.GetMessagesAsync(
                request.Channel.Id,
                request.After,
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

        // Throw if no messages were exported
        if (messageExporter.MessagesExported <= 0)
        {
            throw new DiscordChatExporterException(
                $"Channel '{request.Channel.Name}' (#{request.Channel.Id}) "
                    + $"of guild '{request.Guild.Name}' (#{request.Guild.Id}) "
                    + $"does not contain any matching messages within the specified period."
            );
        }
    }
}
